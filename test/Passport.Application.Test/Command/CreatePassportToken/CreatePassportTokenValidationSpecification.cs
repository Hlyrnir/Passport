using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportToken.Create;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.CreatePassportToken
{
    public sealed class CreatePassportTokenValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public CreatePassportTokenValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Create_ShouldReturnTrue_WhenTokenDoesNotExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppCredentialToAdd = DataFaker.PassportCredential.CreateAtProvider(sProvider: "DEFAULT_UNDEFINED");

            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredentialToVerify, prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            CreatePassportTokenCommand cmdCreate = new CreatePassportTokenCommand()
            {
                CredentialToVerify = ppCredentialToVerify,
                CredentialToAdd = ppCredentialToAdd,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<CreatePassportTokenCommand> hndlValidation = new CreatePassportTokenValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdCreate,
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
        public async Task Create_ShouldReturnMessageError_WhenTokenExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppCredentialToAdd = ppCredentialToVerify;

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredentialToVerify, prvTime.GetUtcNow(), CancellationToken.None);

            CreatePassportTokenCommand cmdCreate = new CreatePassportTokenCommand()
            {
                CredentialToVerify = ppCredentialToVerify,
                CredentialToAdd = ppCredentialToAdd,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<CreatePassportTokenCommand> hndlValidation = new CreatePassportTokenValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdCreate,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Credential {cmdCreate.CredentialToAdd.Credential} at {cmdCreate.CredentialToAdd.Provider} does already exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}