using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.Authentication.ByRefreshToken;

namespace Passport.Application.Test.Command.AuthenticationTokenByRefreshToken
{
    public class AuthenticationTokenByRefreshTokenValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public AuthenticationTokenByRefreshTokenValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task ByCredential_ShouldReturnTrue_WhenCredentialIsValid()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);

            AuthenticationTokenByRefreshTokenCommand cmdToken = new AuthenticationTokenByRefreshTokenCommand()
            {
                PassportId = ppPassport.Id,
                Provider = ppToken.Provider,
                RefreshToken = ppToken.RefreshToken
            };

            IValidation<AuthenticationTokenByRefreshTokenCommand> hndlValidation = new AuthenticationTokenByRefreshTokenValidation(srvValidation: fxtPassport.PassportValidation);

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
