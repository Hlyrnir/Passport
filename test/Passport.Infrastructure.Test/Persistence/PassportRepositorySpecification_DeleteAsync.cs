using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportRepositorySpecification_DeleteAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportRepositorySpecification_DeleteAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenPassportIsDeleted()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);

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
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnRepositoryError_WhenConcurrencyStampIsDifferent()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            PassportTransferObject dtoPassport = ppPassport.MapToTransferObject();
            PassportTransferObject dtoPassportToDelete = dtoPassport.Clone(bResetConcurrencyStamp: true);

            await fxtPassport.PassportRepository.InsertAsync(dtoPassport, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportRepository.DeleteAsync(dtoPassportToDelete, CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not delete passport {dtoPassportToDelete.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(dtoPassport, CancellationToken.None);
        }
    }
}