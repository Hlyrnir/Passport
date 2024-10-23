using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportHolder.Update;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.UpdatePassportHolder
{
    public sealed class UpdatePassportHolderValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportHolderValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportHolderCommand cmdUpdate = new UpdatePassportHolderCommand()
            {
                ConcurrencyStamp = ppHolder.ConcurrencyStamp,
                CultureName = "en-GB",
                EmailAddress = "default@ema.il",
                FirstName = "Jane",
                LastName = "Doe",
                PassportHolderId = ppHolder.Id,
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportHolderCommand> hndlValidation = new UpdatePassportHolderValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
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
        public async Task Update_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            UpdatePassportHolderCommand cmdUpdate = new UpdatePassportHolderCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CultureName = "en-GB",
                EmailAddress = "default@ema.il",
                FirstName = "Jane",
                LastName = "Doe",
                PassportHolderId = Guid.NewGuid(),
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportHolderCommand> hndlValidation = new UpdatePassportHolderValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Passport holder {cmdUpdate.PassportHolderId} does not exist.");

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