using FluentAssertions;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.ConfirmEmailAddress;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.ConfirmEmailAddress
{
    public sealed class ConfirmEmailAddressAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public ConfirmEmailAddressAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            ConfirmEmailAddressCommand msgMessage = new ConfirmEmailAddressCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                EmailAddress = $"{Guid.NewGuid()}@passport.org",
                PassportHolderId = Guid.NewGuid(),
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<ConfirmEmailAddressCommand> msgAuthorization = new ConfirmEmailAddressAuthorization();

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
