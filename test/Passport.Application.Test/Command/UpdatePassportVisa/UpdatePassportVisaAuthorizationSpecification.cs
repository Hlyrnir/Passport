using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportVisa.Update;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.UpdatePassportVisa
{
    public sealed class UpdatePassportVisaAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportVisaAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            UpdatePassportVisaCommand msgMessage = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Level = 0,
                PassportVisaId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<UpdatePassportVisaCommand> msgAuthorization = new UpdatePassportVisaAuthorization();

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