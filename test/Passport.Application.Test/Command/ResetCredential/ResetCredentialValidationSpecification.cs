using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportToken.ResetCredential;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.ResetCredential
{
    public sealed class ResetCredentialValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public ResetCredentialValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenCredentialExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();

            string sInvalidSignature = $"Inval!d_{Guid.NewGuid()}";
            IPassportCredential ppCredentialToApply = DataFaker.PassportCredential.Create(ppCredentialToVerify.Credential, sInvalidSignature);

            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredentialToVerify, prvTime.GetUtcNow(), CancellationToken.None);

            ResetCredentialCommand cmdUpdate = new ResetCredentialCommand()
            {
                CredentialToApply = ppCredentialToApply,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<ResetCredentialCommand> hndlValidation = new ResetCredentialValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
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
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenCredentialDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();

            string sInvalidSignature = $"Inval!d_{Guid.NewGuid()}";
            IPassportCredential ppCredentialToApply = DataFaker.PassportCredential.Create(ppCredentialToVerify.Credential, sInvalidSignature);

            ResetCredentialCommand cmdUpdate = new ResetCredentialCommand()
            {
                CredentialToApply = ppCredentialToApply,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<ResetCredentialCommand> hndlValidation = new ResetCredentialValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Credential {ppCredentialToVerify.Credential} does not exist.");

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