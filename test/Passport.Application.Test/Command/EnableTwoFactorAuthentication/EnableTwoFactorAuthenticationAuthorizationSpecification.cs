using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.EnableTwoFactorAuthentication
{
    public sealed class EnableTwoFactorAuthenticationAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public EnableTwoFactorAuthenticationAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            EnableTwoFactorAuthenticationCommand msgMessage = new EnableTwoFactorAuthenticationCommand()
            {
                CredentialToVerify = ppCredential,
                TwoFactorIsEnabled = false,
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<EnableTwoFactorAuthenticationCommand> msgAuthorization = new EnableTwoFactorAuthenticationAuthorization();

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Update);

            rsltAuthorization.Match(
                msgError => false,
                bResult =>
                {
                    bResult.Should().BeTrue();

                    return true;
                });
        }
    }
}
