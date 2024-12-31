using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_InsertAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_InsertAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnTrue_WhenCredentialIsCreated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltCredential = await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltCredential.Match(
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
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task InsertAsync_ShouldRepositoryError_WhenPassportDoesNotExist()
        {
            // Arrange
            Guid guPassportId = Guid.NewGuid();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(guPassportId);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            // Act
            RepositoryResult<bool> rsltCredential = await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltCredential.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not create token for {guPassportId} at {ppCredential.Provider}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }
    }
}