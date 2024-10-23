using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportHolder.ConfirmPhoneNumber;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.ConfirmPhoneNumber
{
    public sealed class ConfirmPhoneNumberValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public ConfirmPhoneNumberValidationSpecification(PassportFixture fxtPassport)
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

            ConfirmPhoneNumberCommand cmdUpdate = new ConfirmPhoneNumberCommand()
            {
                ConcurrencyStamp = ppHolder.ConcurrencyStamp,
                PassportHolderId = ppHolder.Id,
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<ConfirmPhoneNumberCommand> hndlValidation = new ConfirmPhoneNumberValidation(
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
            ConfirmPhoneNumberCommand cmdUpdate = new ConfirmPhoneNumberCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PassportHolderId = Guid.NewGuid(),
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<ConfirmPhoneNumberCommand> hndlValidation = new ConfirmPhoneNumberValidation(
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