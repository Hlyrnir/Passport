using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportVisa.Delete;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.DeletePassportVisa
{
    public sealed class DeletePassportVisaValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportVisaValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenPassportVisaExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportVisaCommand cmdDelete = new DeletePassportVisaCommand()
            {
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<DeletePassportVisaCommand> hndlValidation = new DeletePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
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
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Delete_ShouldReturnMessageError_WhenPassportVisaDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            DeletePassportVisaCommand cmdDelete = new DeletePassportVisaCommand()
            {
                PassportVisaId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<DeletePassportVisaCommand> hndlValidation = new DeletePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
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
                    msgError.Description.Should().Contain($"Passport visa {cmdDelete.PassportVisaId} does not exist.");

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