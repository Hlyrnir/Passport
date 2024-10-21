using FluentAssertions;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.Update;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.UpdatePassportHolder
{
    public sealed class UpdatePassportHolderAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportHolderAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            UpdatePassportHolderCommand msgMessage = new UpdatePassportHolderCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CultureName = "en-GB",
                EmailAddress = $"{Guid.NewGuid()}@passport.org",
                FirstName = "Jane",
                LastName = "Doe",
                PhoneNumber = "000",
                PassportHolderId = Guid.NewGuid(),
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<UpdatePassportHolderCommand> msgAuthorization = new UpdatePassportHolderAuthorization();

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