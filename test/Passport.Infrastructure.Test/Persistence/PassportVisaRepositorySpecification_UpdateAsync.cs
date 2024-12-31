using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_UpdateAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_UpdateAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenVisaIsUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            PassportVisaTransferObject dtoVisa = ppVisa.MapToTransferObject();
            PassportVisaTransferObject dtoVisaToUpdate = dtoVisa.Clone(bResetConcurrencyStamp: false);

            await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisa, prvTime.GetUtcNow(), CancellationToken.None);

            Domain.Aggregate.PassportVisa? ppVisaToUpdate = dtoVisaToUpdate.Initialize();

            if (ppVisaToUpdate is null)
                throw new NullReferenceException();

            // Act
            bool bLevelHasChanged = ppVisaToUpdate.TryChangeLevel(1);

            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportVisaRepository.UpdateAsync(ppVisaToUpdate.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            bLevelHasChanged.Should().BeTrue();

            rsltUpdate.Match(
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
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByIdAsync(ppVisaToUpdate.Id, CancellationToken.None);

            await rsltVisa.MatchAsync(
                msgError => false,
                async dtoVisa =>
                {
                    await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisa, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldChangeConcurrencyStamp_WhenPassportIsUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            PassportVisaTransferObject dtoVisa = ppVisa.MapToTransferObject();
            PassportVisaTransferObject dtoVisaToUpdate = dtoVisa.Clone(bResetConcurrencyStamp: false);

            await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisa, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            await fxtPassport.PassportVisaRepository.UpdateAsync(dtoVisaToUpdate, prvTime.GetUtcNow(), CancellationToken.None);

            RepositoryResult<PassportVisaTransferObject> rsltUpdate = await fxtPassport.PassportVisaRepository.FindByIdAsync(dtoVisaToUpdate.Id, CancellationToken.None);

            // Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportVisaInRepository =>
                {
                    dtoPassportVisaInRepository.Should().NotBeNull();
                    dtoPassportVisaInRepository.ConcurrencyStamp.Should().NotBe(dtoVisaToUpdate.ConcurrencyStamp);
                    dtoPassportVisaInRepository.Id.Should().Be(dtoVisaToUpdate.Id);
                    dtoPassportVisaInRepository.Level.Should().Be(dtoVisaToUpdate.Level);
                    dtoPassportVisaInRepository.Name.Should().Be(dtoVisaToUpdate.Name);

                    return true;
                });

            // Clean up
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByIdAsync(dtoVisa.Id, CancellationToken.None);

            await rsltVisa.MatchAsync(
                msgError => false,
                async dtoVisa =>
                {
                    await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisa, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenConcurrencyStampIsDifferent()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            PassportVisaTransferObject dtoVisa = ppVisa.MapToTransferObject();
            PassportVisaTransferObject dtoVisaToUpdate = dtoVisa.Clone(bResetConcurrencyStamp: true);

            await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisa, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportVisaRepository.UpdateAsync(dtoVisaToUpdate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not update visa {dtoVisaToUpdate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByIdAsync(dtoVisa.Id, CancellationToken.None);

            await rsltVisa.MatchAsync(
                msgError => false,
                async dtoVisa =>
                {
                    await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisa, CancellationToken.None);
                    return true;
                });
        }
    }
}