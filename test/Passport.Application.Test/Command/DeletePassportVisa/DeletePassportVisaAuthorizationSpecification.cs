using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportVisa.Delete;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.DeletePassportVisa
{
    public sealed class DeletePassportVisaAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportVisaAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            DeletePassportVisaCommand msgMessage = new DeletePassportVisaCommand()
            {
                PassportVisaId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<DeletePassportVisaCommand> msgAuthorization = new DeletePassportVisaAuthorization();

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Delete);

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