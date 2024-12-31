using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_ResetRefreshTokenAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_ResetRefreshTokenAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Reset_ShouldReturnTrue_WhenRefreshTokenIsReset()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            DateTimeOffset dtResetAt = prvTime.GetUtcNow();
            RepositoryResult<bool> rsltToken = await fxtPassport.PassportTokenRepository.ResetRefreshTokenAsync(ppPassport.Id, ppCredential.Provider, dtResetAt, CancellationToken.None);

            // Assert
            await rsltToken.MatchAsync(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                async bResult =>
                {
                    bResult.Should().BeTrue();

                    RepositoryResult<PassportTokenTransferObject> rsltToken = await fxtPassport.PassportTokenRepository.FindTokenByCredentialAsync(ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

                    return rsltToken.Match(
                        msgError => false,
                        dtoPassportTokenInRepository =>
                        {
                            dtoPassportTokenInRepository.ExpiredAt.Should().Be(dtResetAt.Add(fxtPassport.PassportSetting.RefreshTokenExpiresAfterDuration));
                            dtoPassportTokenInRepository.RefreshToken.Should().NotBe(ppToken.RefreshToken);
                            return true;
                        });
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Reset_ShouldReturnRepositoryError_WhenProviderDoesNotMatch()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            string sInvalidProvider = "INVALID_PROVIDER";

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            DateTimeOffset dtResetAt = prvTime.GetUtcNow();
            RepositoryResult<bool> rsltToken = await fxtPassport.PassportTokenRepository.ResetRefreshTokenAsync(ppPassport.Id, sInvalidProvider, dtResetAt, CancellationToken.None);

            // Assert
            rsltToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Refresh token has not been reset at provider {sInvalidProvider}.");

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