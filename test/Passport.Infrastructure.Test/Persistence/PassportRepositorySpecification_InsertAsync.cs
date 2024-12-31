using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportRepositorySpecification_InsertAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportRepositorySpecification_InsertAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnTrue_WhenPassportIsCreated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltPassport.Match(
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
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnRepositoryError_WhenPassportIdExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not create passport {ppPassport.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task InsertAsync_ShouldContainPassportVisaId_WhenPassportVisaIsAdded()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

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
                    dtoPassportInRepository.Should().BeEquivalentTo(ppPassport, options => options.Excluding(x => x.ConcurrencyStamp));
                    dtoPassportInRepository.VisaId.Should().Contain(ppVisa.Id);

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