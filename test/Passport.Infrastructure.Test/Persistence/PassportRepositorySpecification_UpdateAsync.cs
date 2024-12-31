using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportRepositorySpecification_UpdateAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportRepositorySpecification_UpdateAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIsUpdated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            ppPassport.TryDisable(ppPassport, prvTime.GetUtcNow());

            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportRepository.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
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
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            await rsltPassport.MatchAsync<bool>(
                msgError => false,
                async dtoPassportToDelete =>
                {
                    await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportWithVisaIsUpdated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            ppPassport.TryAddVisa(ppVisa);

            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportRepository.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
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
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            await rsltPassport.MatchAsync<bool>(
                msgError => false,
                async dtoPassportToDelete =>
                {
                    await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);
                    return true;
                });

            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldChangeConcurrencyStamp_WhenPassportIsUpdated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            ppPassport.TryDisable(ppPassport, prvTime.GetUtcNow());

            await fxtPassport.PassportRepository.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            
            RepositoryResult<PassportTransferObject> rsltUpdate = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

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
                    dtoPassportInRepository.ConcurrencyStamp.Should().NotBe(ppPassport.ConcurrencyStamp);
                    dtoPassportInRepository.ExpiredAt.Should().Be(ppPassport.ExpiredAt);
                    dtoPassportInRepository.HolderId.Should().Be(ppPassport.HolderId);
                    dtoPassportInRepository.Id.Should().Be(ppPassport.Id);
                    dtoPassportInRepository.IssuedBy.Should().Be(ppPassport.IssuedBy);
                    dtoPassportInRepository.LastCheckedAt.Should().Be(ppPassport.LastCheckedAt);
                    dtoPassportInRepository.LastCheckedBy.Should().Be(ppPassport.LastCheckedBy);
                    return true;
                });

            // Clean up
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            await rsltPassport.MatchAsync<bool>(
                msgError => false,
                async dtoPassportToDelete =>
                {
                    await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenConcurrencyStampIsDifferent()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            PassportTransferObject dtoPassport = ppPassport.MapToTransferObject();
            PassportTransferObject dtoPassportToUpdate = dtoPassport.Clone(bResetConcurrencyStamp: true);

            await fxtPassport.PassportRepository.InsertAsync(dtoPassport, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltUpdate = await fxtPassport.PassportRepository.UpdateAsync(dtoPassportToUpdate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltUpdate.Match<bool>(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not update passport {dtoPassportToUpdate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(dtoPassport.Id, CancellationToken.None);

            await rsltPassport.MatchAsync<bool>(
                msgError => false,
                async dtoPassportToDelete =>
                {
                    await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);
                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldNotContainVisaId_WhenVisaIsRemoved()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_01 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_02 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_03 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_04 = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_01.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_02.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_03.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_04.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            ppPassport.TryAddVisa(ppVisa_01);
            ppPassport.TryAddVisa(ppVisa_02);
            ppPassport.TryAddVisa(ppVisa_03);
            ppPassport.TryAddVisa(ppVisa_04);

            await fxtPassport.PassportRepository.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            await rsltPassport.MatchAsync<bool>(
                msgError => false,
                async dtoPassportInRepository =>
                {
                    Domain.Aggregate.Passport? ppPassport = dtoPassportInRepository.Initialize();

                    if (ppPassport is null)
                        return false;

                    ppPassport.TryRemoveVisa(ppVisa_01);

                    await fxtPassport.PassportRepository.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

                    return true;
                });

            // Assert
            RepositoryResult<PassportTransferObject> rsltUpdate = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            rsltUpdate.Match<bool>(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().NotBeNull();
                    dtoPassportInRepository.VisaId.Should().NotContain(ppVisa_01.Id);
                    dtoPassportInRepository.VisaId.Should().Contain(ppVisa_02.Id);
                    dtoPassportInRepository.VisaId.Should().Contain(ppVisa_03.Id);
                    dtoPassportInRepository.VisaId.Should().Contain(ppVisa_04.Id);

                    return true;
                });

            // Clean up
            RepositoryResult<PassportTransferObject> rsltPassportToDelete = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            await rsltPassportToDelete.MatchAsync<bool>(
                msgError => false,
                async dtoPassportToDelete =>
                {
                    await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);
                    return true;
                });

            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_01.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_02.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_03.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_04.MapToTransferObject(), CancellationToken.None);
        }
    }
}
