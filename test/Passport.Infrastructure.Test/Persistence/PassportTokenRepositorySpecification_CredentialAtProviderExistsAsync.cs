using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_CredentialAtProviderExistsAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_CredentialAtProviderExistsAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Exists_ShouldReturnTrue_WhenCredentialExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltCredential = await fxtPassport.PassportTokenRepository.CredentialAtProviderExistsAsync(ppCredential.Credential, ppCredential.Provider, CancellationToken.None);

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
        public async Task Exists_ShouldReturnFalse_WhenCredentialDoesNotExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltCredential = await fxtPassport.PassportTokenRepository.CredentialAtProviderExistsAsync(ppCredential.Credential, ppCredential.Provider, CancellationToken.None);

            // Assert
            rsltCredential.Match(
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
    }
}