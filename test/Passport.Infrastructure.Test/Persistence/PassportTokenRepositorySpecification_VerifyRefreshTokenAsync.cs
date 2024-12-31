using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_VerifyRefreshTokenAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_VerifyRefreshTokenAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Verify_ShouldReturnAllowedAccessAttempt_WhenRefreshTokenMatches()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<int> rsltVerify = await fxtPassport.PassportTokenRepository.VerifyRefreshTokenAsync(ppPassport.Id, ppCredential.Provider, ppToken.RefreshToken, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltVerify.Match(
                msgError => false,
                iAllowedAccessAttempt =>
                {
                    iAllowedAccessAttempt.Should().Be(fxtPassport.PassportSetting.MaximalAllowedAccessAttempt);
                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Verify_ShouldReturnDecrementedAllowedAccessAttempt_WhenRefreshTokenDoesNotMatch()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            string sInvalidRefreshToken = Guid.NewGuid().ToString();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<int> rsltVerify = await fxtPassport.PassportTokenRepository.VerifyRefreshTokenAsync(ppPassport.Id, ppCredential.Provider, sInvalidRefreshToken, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltVerify.Match(
                msgError => false,
                iAllowedAccessAttempt =>
                {
                    iAllowedAccessAttempt.Should().Be(fxtPassport.PassportSetting.MaximalAllowedAccessAttempt + (-1));
                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Verify_ShouldReturnDecrementedAllowedAccessAttempt_WhenRefreshTokenIsExpired()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            PassportTokenTransferObject dtoToken = new PassportTokenTransferObject()
            {
                ExpiredAt = DateTimeOffset.MinValue,
                Id = Guid.NewGuid(),
                PassportId = ppPassport.Id,
                Provider = ppCredential.Provider,
                RefreshToken = Guid.NewGuid().ToString(),
                TwoFactorIsEnabled = false
            };

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(dtoToken, ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<int> rsltVerify = await fxtPassport.PassportTokenRepository.VerifyRefreshTokenAsync(ppPassport.Id, ppCredential.Provider, dtoToken.RefreshToken, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltVerify.Match(
                msgError => false,
                iAllowedAccessAttempt =>
                {
                    iAllowedAccessAttempt.Should().Be(fxtPassport.PassportSetting.MaximalAllowedAccessAttempt + (-1));
                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(dtoToken, CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}