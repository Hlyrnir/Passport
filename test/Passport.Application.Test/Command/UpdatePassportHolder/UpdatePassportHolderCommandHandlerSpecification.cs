using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.Update;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.UpdatePassportHolder
{
    public class UpdatePassportHolderCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportHolderCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportHolderIsUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtPassport.PassportSetting);

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

            UpdatePassportHolderCommandHandler hdlCommand = new UpdatePassportHolderCommandHandler(
                prvTime: prvTime,
                repoHolder: fxtPassport.PassportHolderRepository,
                ppSetting: fxtPassport.PassportSetting);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            await rsltUpdate.MatchAsync(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                async bResult =>
                {
                    bResult.Should().BeTrue();

                    RepositoryResult<PassportHolderTransferObject> rsltHolder = await fxtPassport.PassportHolderRepository.FindByIdAsync(ppHolder.Id, CancellationToken.None);

                    return rsltHolder.Match(
                        msgError => false,
                        dtoHolderInRepository =>
                        {
                            dtoHolderInRepository.CultureName.Should().Be(cmdUpdate.CultureName);
                            dtoHolderInRepository.EmailAddress.Should().Be(cmdUpdate.EmailAddress);
                            dtoHolderInRepository.EmailAddressIsConfirmed.Should().BeFalse();
                            dtoHolderInRepository.FirstName.Should().Be(cmdUpdate.FirstName);
                            dtoHolderInRepository.LastName.Should().Be(cmdUpdate.LastName);
                            dtoHolderInRepository.PhoneNumber.Should().Be(cmdUpdate.PhoneNumber);
                            dtoHolderInRepository.PhoneNumberIsConfirmed.Should().BeFalse();

                            return true;
                        });
                });

            //Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnRepositoryError_WhenConcurrencyStampDoNotMatch()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtPassport.PassportSetting);

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            string sObsoleteConcurrencyStamp = Guid.NewGuid().ToString();

            UpdatePassportHolderCommand cmdUpdate = new UpdatePassportHolderCommand()
            {
                ConcurrencyStamp = sObsoleteConcurrencyStamp,
                CultureName = "en-GB",
                EmailAddress = "default@ema.il",
                FirstName = "Jane",
                LastName = "Doe",
                PassportHolderId = ppHolder.Id,
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            UpdatePassportHolderCommandHandler hdlCommand = new UpdatePassportHolderCommandHandler(
                prvTime: prvTime,
                repoHolder: fxtPassport.PassportHolderRepository,
                ppSetting: fxtPassport.PassportSetting);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(DefaultMessageError.ConcurrencyViolation);

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });

            //Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }
    }
}