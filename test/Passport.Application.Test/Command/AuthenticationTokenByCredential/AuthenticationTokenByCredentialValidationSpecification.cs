using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.Authentication.ByCredential;

namespace Passport.Application.Test.Command.AuthenticationTokenByCredential
{
    public class AuthenticationTokenByCredentialValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public AuthenticationTokenByCredentialValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task ByCredential_ShouldReturnTrue_WhenCredentialIsValid()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppCredential
            };

            IValidation<AuthenticationTokenByCredentialCommand> hndlValidation = new AuthenticationTokenByCredentialValidation(srvValidation: fxtPassport.PassportValidation);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdToken,
                tknCancellation: CancellationToken.None);

            //Assert
            rsltValidation.Match(
                msgError =>
                {
                    return false;
                },
                bResult =>
                {
                    bResult.Should().Be(true);
                    return true;
                });
        }
    }
}
