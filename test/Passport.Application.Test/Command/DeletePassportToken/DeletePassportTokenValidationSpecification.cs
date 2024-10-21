using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportToken.Delete;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.DeletePassportToken
{
    public sealed class DeletePassportTokenValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportTokenValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenCredentialExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportTokenCommand cmdDelete = new DeletePassportTokenCommand()
            {
                CredentialToRemove = ppCredential,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<DeletePassportTokenCommand> hndlValidation = new DeletePassportTokenValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdDelete,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeTrue();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Delete_ShouldReturnMessageError_WhenCredentialDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            DeletePassportTokenCommand cmdDelete = new DeletePassportTokenCommand()
            {
                CredentialToRemove = ppCredential,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<DeletePassportTokenCommand> hndlValidation = new DeletePassportTokenValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdDelete,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Credential at provider {ppCredential.Provider} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }
    }
}