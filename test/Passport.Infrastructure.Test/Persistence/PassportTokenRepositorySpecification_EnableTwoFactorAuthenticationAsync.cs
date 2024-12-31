using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_EnableTwoFactorAuthenticationAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_EnableTwoFactorAuthenticationAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Enable_ShouldReturnTrue_WhenTwoFactorAuthenticationIsEnabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltToken = await fxtPassport.PassportTokenRepository.EnableTwoFactorAuthenticationAsync(ppToken.MapToTransferObject(), true, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltToken.Match(
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
        public async Task Enable_ShouldReturnTrue_WhenTwoFactorAuthenticationIsDisabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltToken = await fxtPassport.PassportTokenRepository.EnableTwoFactorAuthenticationAsync(ppToken.MapToTransferObject(), false, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltToken.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Enable_ShouldReturnRepositoryError_WhenTokenDoesNotMatch()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            Domain.Aggregate.PassportToken ppInvalidToken = DataFaker.PassportToken.CreateDefault(Guid.NewGuid());

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltToken = await fxtPassport.PassportTokenRepository.EnableTwoFactorAuthenticationAsync(ppInvalidToken.MapToTransferObject(), bIsEnabled: true, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Two factor authentication is enabled: {ppToken.TwoFactorIsEnabled}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}