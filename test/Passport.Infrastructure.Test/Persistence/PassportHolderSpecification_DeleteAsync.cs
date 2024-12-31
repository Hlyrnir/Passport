using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportHolderRepositorySpecification_DeleteAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportHolderRepositorySpecification_DeleteAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenHolderIsDeleted()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);

            // Assert
            rsltPassport.Match<bool>(
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
        }

        [Fact]
        public async Task Delete_ShouldReturnRepositoryError_WhenConcurrencyStampIsDifferent()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            PassportHolderTransferObject dtoHolder = ppHolder.MapToTransferObject();
            PassportHolderTransferObject dtoHolderToDelete = dtoHolder.Clone(bResetConcurrencyStamp: true);

            await fxtPassport.PassportHolderRepository.InsertAsync(dtoHolder, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportHolderRepository.DeleteAsync(dtoHolderToDelete, CancellationToken.None);

            // Assert
            rsltPassport.Match<bool>(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not delete holder {dtoHolderToDelete.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(dtoHolder, CancellationToken.None);
        }
    }
}