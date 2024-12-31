using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_VerifyCredentialAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_VerifyCredentialAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Verify_ShouldReturnAllowedAccessAttempt_WhenCredentialMatches()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<int> rsltVerify = await fxtPassport.PassportTokenRepository.VerifyCredentialAsync(ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

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
        public async Task Verify_ShouldReturnDecrementedAllowedAccessAttempt_WhenSignatureDoesNotMatch()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppInvalidCredential = DataFaker.PassportCredential.Create(ppCredential.Credential, Guid.NewGuid().ToString());

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<int> rsltVerify = await fxtPassport.PassportTokenRepository.VerifyCredentialAsync(ppInvalidCredential, prvTime.GetUtcNow(), CancellationToken.None);

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
    }
}