using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportHolderRepositorySpecification_UpdateAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportHolderRepositorySpecification_UpdateAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIsUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            PassportHolderTransferObject dtoHolder = ppHolder.MapToTransferObject();
            PassportHolderTransferObject dtoHolderToUpdate = dtoHolder.Clone(bResetConcurrencyStamp: false);

            Domain.Aggregate.PassportHolder? ppHolderToUpdate = dtoHolderToUpdate.Initialize(DataFaker.PassportHolder.Setting);

            if (ppHolderToUpdate is null)
                throw new NullReferenceException();

            await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolder, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            bool bEmailAddressHasChanged=ppHolderToUpdate.TryChangeEmailAddress(sEmailAddress: "another@passport.org", ppSetting: DataFaker.PassportHolder.Setting);

            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportHolderRepository.UpdateAsync(ppHolderToUpdate.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            bEmailAddressHasChanged.Should().BeTrue();

            rsltUpdate.Match<bool>(
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
            RepositoryResult<PassportHolderTransferObject> rsltHolder = await fxtPassport.PassportHolderRepository.FindByIdAsync(ppHolder.Id, CancellationToken.None);

            await rsltHolder.MatchAsync(
                msgError => false,
                async dtoHolderToDelete =>
                {
                    await fxtPassport.PassportHolderRepository.DeleteAsync(dtoHolderToDelete, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldChangeConcurrencyStamp_WhenPassportIsUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            PassportHolderTransferObject dtoHolder = ppHolder.MapToTransferObject();
            PassportHolderTransferObject dtoHolderToUpdate = dtoHolder.Clone(bResetConcurrencyStamp: false);

            await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolder, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            await fxtPassport.PassportHolderRepository.UpdateAsync(dtoHolderToUpdate, prvTime.GetUtcNow(), CancellationToken.None);

            RepositoryResult<PassportHolderTransferObject> rsltUpdate = await fxtPassport.PassportHolderRepository.FindByIdAsync(dtoHolderToUpdate.Id, CancellationToken.None);

            // Assert
            rsltUpdate.Match<bool>(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().NotBeNull();
                    dtoPassportInRepository.ConcurrencyStamp.Should().NotBe(dtoHolderToUpdate.ConcurrencyStamp);
                    dtoPassportInRepository.CultureName.Should().Be(dtoHolderToUpdate.CultureName);
                    dtoPassportInRepository.EmailAddress.Should().Be(dtoHolderToUpdate.EmailAddress);
                    dtoPassportInRepository.EmailAddressIsConfirmed.Should().Be(dtoHolderToUpdate.EmailAddressIsConfirmed);
                    dtoPassportInRepository.FirstName.Should().Be(dtoHolderToUpdate.FirstName);
                    dtoPassportInRepository.Id.Should().Be(dtoHolderToUpdate.Id);
                    dtoPassportInRepository.LastName.Should().Be(dtoHolderToUpdate.LastName);
                    dtoPassportInRepository.PhoneNumber.Should().Be(dtoHolderToUpdate.PhoneNumber);
                    dtoPassportInRepository.PhoneNumberIsConfirmed.Should().Be(dtoHolderToUpdate.PhoneNumberIsConfirmed);
                    dtoPassportInRepository.SecurityStamp.Should().Be(dtoHolderToUpdate.SecurityStamp);
                    return true;
                });

            // Clean up
            RepositoryResult<PassportHolderTransferObject> rsltHolder = await fxtPassport.PassportHolderRepository.FindByIdAsync(dtoHolder.Id, CancellationToken.None);

            await rsltHolder.MatchAsync(
                msgError => false,
                async ppHolderToDelete =>
                {
                    await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolderToDelete, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenConcurrencyStampIsDifferent()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            PassportHolderTransferObject dtoHolder = ppHolder.MapToTransferObject();
            PassportHolderTransferObject dtoHolderToUpdate = dtoHolder.Clone(bResetConcurrencyStamp: true);

            await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolder, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportHolderRepository.UpdateAsync(dtoHolderToUpdate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltUpdate.Match<bool>(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not update holder {dtoHolderToUpdate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            RepositoryResult<PassportHolderTransferObject> rsltHolder = await fxtPassport.PassportHolderRepository.FindByIdAsync(dtoHolder.Id, CancellationToken.None);

            await rsltHolder.MatchAsync(
                msgError => false,
                async ppHolderToDelete =>
                {
                    await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolderToDelete, CancellationToken.None);
                    return true;
                });
        }
    }
}