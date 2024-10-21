using FluentAssertions;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.ConfirmPhoneNumber;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.ConfirmPhoneNumber
{
    public sealed class ConfirmPhoneNumberAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public ConfirmPhoneNumberAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            ConfirmPhoneNumberCommand msgMessage = new ConfirmPhoneNumberCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PassportHolderId = Guid.NewGuid(),
                PhoneNumber = "000",
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<ConfirmPhoneNumberCommand> msgAuthorization = new ConfirmPhoneNumberAuthorization();

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
