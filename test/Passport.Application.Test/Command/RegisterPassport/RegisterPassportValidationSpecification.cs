using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.Passport.Register;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.RegisterPassport
{
    public sealed class RegisterPassportValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public RegisterPassportValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Register_ShouldReturnTrue_WhenCredentialDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            RegisterPassportCommand cmdRegister = new RegisterPassportCommand()
            {
                CredentialToRegister = ppCredential,
                CultureName = "en-GB",
                EmailAddress = "default@ema.il",
                FirstName = "Jane",
                LastName = "Doe",
                IssuedBy = Guid.NewGuid(),
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<RegisterPassportCommand> hndlValidation = new RegisterPassportValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdRegister,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().BeNull();
                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeTrue();
                    return true;
                });
        }

        [Fact]
        public async Task Register_ShouldReturnMessageError_WhenCredentialDoesExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            RegisterPassportCommand cmdRegister = new RegisterPassportCommand()
            {
                CredentialToRegister = ppCredential,
                CultureName = "en-GB",
                EmailAddress = "default@ema.il",
                FirstName = "Jane",
                LastName = "Doe",
                IssuedBy = Guid.NewGuid(),
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<RegisterPassportCommand> hndlValidation = new RegisterPassportValidation(
                repoToken: fxtPassport.PassportTokenRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdRegister,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Credential {cmdRegister.CredentialToRegister.Credential} does already exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();
                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}
