using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Update;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.UpdatePassport
{
    public sealed class UpdatePassportAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            UpdatePassportCommand msgMessage = new UpdatePassportCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                ExpiredAt = prvTime.GetUtcNow(),
                IsEnabled = false,
                IsAuthority = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = Guid.NewGuid(),
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<UpdatePassportCommand> msgAuthorization = new UpdatePassportAuthorization();

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