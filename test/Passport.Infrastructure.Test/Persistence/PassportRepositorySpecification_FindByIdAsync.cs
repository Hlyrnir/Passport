using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportRepositorySpecification_FindByIdAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportRepositorySpecification_FindByIdAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task FindById_ShouldFindPassport_WhenPassportIdExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().NotBeNull();
                    dtoPassportInRepository.Should().BeEquivalentTo(ppPassport);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task FindById_ShouldReturnRepositoryError_WhenPassportIdDoesNotExist()
        {
            // Arrange
            Guid guPassportId = Guid.NewGuid();

            // Act
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(guPassportId, CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"No data for {guPassportId} has been found.");

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().BeNull();

                    return true;
                });
        }

        [Fact]
        public async Task FindById_VisaIdShouldNotBeEmpty_WhenVisaExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            Guid guPassportToDelete = Guid.Empty;

            // Act
            ppPassport.TryAddVisa(ppVisa);
            await fxtPassport.PassportRepository.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().NotBeNull();
                    dtoPassportInRepository.VisaId.Should().NotBeEmpty();

                    return true;
                });

            // Clean up
            RepositoryResult<PassportTransferObject> rsltPassportToDelete = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            await rsltPassportToDelete.MatchAsync(
                msgError => false,
                async dtoPassportToDelete =>
                {
                    await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);

                    return true;
                });

            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }
    }
}