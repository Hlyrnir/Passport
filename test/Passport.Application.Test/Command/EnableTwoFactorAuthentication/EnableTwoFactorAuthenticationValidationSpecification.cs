using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.EnableTwoFactorAuthentication
{
    public sealed class EnableTwoFactorAuthenticationValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public EnableTwoFactorAuthenticationValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.Create("default@ema.il", "$ignatUr3");

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            EnableTwoFactorAuthenticationCommand cmdUpdate = new EnableTwoFactorAuthenticationCommand()
            {
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid(),
                TwoFactorIsEnabled = true
            };

            IValidation<EnableTwoFactorAuthenticationCommand> hndlValidation = new EnableTwoFactorAuthenticationValidation(
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
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenTokenDoesNotExist()
        {
            // Arrange
            EnableTwoFactorAuthenticationCommand cmdUpdate = new EnableTwoFactorAuthenticationCommand()
            {
                CredentialToVerify = DataFaker.PassportCredential.Create("default@ema.il", "$ignatUr3"),
                RestrictedPassportId = Guid.NewGuid(),
                TwoFactorIsEnabled = true
            };

            IValidation<EnableTwoFactorAuthenticationCommand> hndlValidation = new EnableTwoFactorAuthenticationValidation(
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
                    msgError.Description.Should().Contain($"Credential {cmdUpdate.CredentialToVerify.Credential} does not exist.");

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