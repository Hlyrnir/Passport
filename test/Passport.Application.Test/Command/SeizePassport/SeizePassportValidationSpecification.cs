using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.Passport.Seize;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.SeizePassport
{
    public sealed class SeizePassportValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public SeizePassportValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            SeizePassportCommand cmdDelete = new SeizePassportCommand()
            {
                PassportIdToSeize = ppPassport.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<SeizePassportCommand> hndlValidation = new SeizePassportValidation(
                repoPassport: fxtPassport.PassportRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdDelete,
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

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Delete_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            SeizePassportCommand cmdDelete = new SeizePassportCommand()
            {
                PassportIdToSeize = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<SeizePassportCommand> hndlValidation = new SeizePassportValidation(
                repoPassport: fxtPassport.PassportRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdDelete,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Passport {cmdDelete.PassportIdToSeize} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }
    }
}