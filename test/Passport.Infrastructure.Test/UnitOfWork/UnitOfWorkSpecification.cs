using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.UnitOfWork
{
    public class UnitOfWorkSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public UnitOfWorkSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldCreatePassport_WhenTransactionIsCommitted()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            bool bIsCommitted = false;
            bool bIsRolledBack = false;

            // Act
            await fxtPassport.UnitOfWork.TransactionAsync(async () =>
            {
                await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
                await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

                bIsCommitted = fxtPassport.UnitOfWork.TryCommit();

                if (bIsCommitted == false)
                    bIsRolledBack = fxtPassport.UnitOfWork.TryRollback();
            });

            // Assert
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            bIsCommitted.Should().BeTrue();
            bIsRolledBack.Should().BeFalse();

            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().BeEquivalentTo(ppPassport);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldNotCreatePassport_WhenTransactionIsRolledBack()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            bool bIsCommitted = false;
            bool bIsRolledBack = false;

            // Act
            await fxtPassport.UnitOfWork.TransactionAsync(async () =>
            {
                await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
                await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

                bIsRolledBack = fxtPassport.UnitOfWork.TryRollback();
            });

            // Assert
            RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

            bIsCommitted.Should().BeFalse();
            bIsRolledBack.Should().BeTrue();

            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"No data for {ppPassport.Id} has been found.");

                    return false;
                },
                dtoPassportInRepository =>
                {
                    dtoPassportInRepository.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }
    }
}
