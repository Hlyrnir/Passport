using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportHolder.Delete;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.DeletePassportHolder
{
    public sealed class DeletePassportHolderValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportHolderValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenHolderExists()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportHolderCommand cmdDelete = new DeletePassportHolderCommand()
            {
                PassportHolderId = ppHolder.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<DeletePassportHolderCommand> hndlValidation = new DeletePassportHolderValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
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
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Delete_ShouldReturnMessageError_WhenHolderDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            DeletePassportHolderCommand cmdDelete = new DeletePassportHolderCommand()
            {
                PassportHolderId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<DeletePassportHolderCommand> hndlValidation = new DeletePassportHolderValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
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
                    msgError.Description.Should().Contain($"Passport holder {cmdDelete.PassportHolderId} does not exist.");

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