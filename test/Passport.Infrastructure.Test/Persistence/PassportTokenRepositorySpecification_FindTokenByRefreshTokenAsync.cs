using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_FindTokenByRefreshTokenAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_FindTokenByRefreshTokenAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task FindByRefreshToken_ShouldFindToken_WhenRefreshTokenExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<PassportTokenTransferObject> rsltTokenToVerify = await fxtPassport.PassportTokenRepository.FindTokenByRefreshTokenAsync(ppPassport.Id, ppToken.Provider, ppToken.RefreshToken, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltTokenToVerify.Match(
                msgError => false,
                dtoPassportTokenInRepository =>
                {
                    dtoPassportTokenInRepository.Should().NotBeNull();
                    dtoPassportTokenInRepository.PassportId.Should().Be(ppPassport.Id);
                    dtoPassportTokenInRepository.Provider.Should().Be(ppCredential.Provider);
                    dtoPassportTokenInRepository.ExpiredAt.Should().NotBe(ppToken.ExpiredAt);
                    dtoPassportTokenInRepository.RefreshToken.Should().NotBe(ppToken.RefreshToken);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task FindByRefreshToken_ShouldReturnRepositoryError_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            string sInvalidRefreshToken = Guid.NewGuid().ToString();

            // Act
            RepositoryResult<PassportTokenTransferObject> rsltToken = await fxtPassport.PassportTokenRepository.FindTokenByRefreshTokenAsync(ppPassport.Id, ppCredential.Provider, sInvalidRefreshToken, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Refresh token does not match at provider {ppCredential.Provider}.");

                    return false;
                },
                dtoPassportTokenInRepository =>
                {
                    dtoPassportTokenInRepository.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}
