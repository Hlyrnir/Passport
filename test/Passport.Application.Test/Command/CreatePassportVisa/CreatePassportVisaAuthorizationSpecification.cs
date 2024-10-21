using FluentAssertions;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportVisa.Create;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.CreatePassportVisa
{
    public sealed class CreatePassportVisaAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public CreatePassportVisaAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Create_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            CreatePassportVisaCommand msgMessage = new CreatePassportVisaCommand()
            {
                Name = Guid.NewGuid().ToString(),
                Level = 0,
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<CreatePassportVisaCommand> msgAuthorization = new CreatePassportVisaAuthorization();

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Create);

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