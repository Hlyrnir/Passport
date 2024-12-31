using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportHolderRepositorySpecification_FindAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportHolderRepositorySpecification_FindAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task FindById_ShouldFindPassport_WhenPassportIdExists()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<PassportHolderTransferObject> rsltHolder = await fxtPassport.PassportHolderRepository.FindByIdAsync(ppHolder.Id, CancellationToken.None);

            // Assert
            rsltHolder.Match<bool>(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportHolderInRepository =>
                {
                    dtoPassportHolderInRepository.Should().NotBeNull();
                    dtoPassportHolderInRepository.Should().BeEquivalentTo(ppHolder);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task FindById_ShouldReturnRepositoryError_WhenPassportIdDoesNotExist()
        {
            // Arrange
            Guid guHolderId = Guid.NewGuid();

            // Act
            RepositoryResult<PassportHolderTransferObject> rsltPassport = await fxtPassport.PassportHolderRepository.FindByIdAsync(guHolderId, CancellationToken.None);

            // Assert
            rsltPassport.Match<bool>(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"No data for {guHolderId} has been found.");

                    return false;
                },
                dtoPassportHolderInRepository =>
                {
                    dtoPassportHolderInRepository.Should().BeNull();

                    return true;
                });
        }
    }
}