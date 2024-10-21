using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.ConfirmEmailAddress;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.ConfirmEmailAddress
{
    public class ConfirmEmailAddressCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public ConfirmEmailAddressCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenEmailAddressIsConfirmed()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtPassport.PassportSetting);

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            ConfirmEmailAddressCommand cmdUpdate = new ConfirmEmailAddressCommand()
            {
                ConcurrencyStamp = ppHolder.ConcurrencyStamp,
                EmailAddress = ppHolder.EmailAddress,
                PassportHolderId = ppHolder.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            ConfirmEmailAddressCommandHandler hdlCommand = new ConfirmEmailAddressCommandHandler(
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
                            dtoHolderInRepository.CultureName.Should().Be(ppHolder.CultureName);
                            dtoHolderInRepository.EmailAddress.Should().Be(ppHolder.EmailAddress);
                            dtoHolderInRepository.EmailAddressIsConfirmed.Should().BeTrue();
                            dtoHolderInRepository.FirstName.Should().Be(ppHolder.FirstName);
                            dtoHolderInRepository.LastName.Should().Be(ppHolder.LastName);
                            dtoHolderInRepository.PhoneNumber.Should().Be(ppHolder.PhoneNumber);
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

            ConfirmEmailAddressCommand cmdUpdate = new ConfirmEmailAddressCommand()
            {
                ConcurrencyStamp = sObsoleteConcurrencyStamp,
                EmailAddress = ppHolder.EmailAddress,
                PassportHolderId = ppHolder.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            ConfirmEmailAddressCommandHandler hdlCommand = new ConfirmEmailAddressCommandHandler(
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
