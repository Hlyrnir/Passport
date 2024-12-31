using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportTokenRepositorySpecification_ResetCredentialAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportTokenRepositorySpecification_ResetCredentialAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Reset_ShouldReturnTrue_WhenCredentialIsReset()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppCredentialToApply = DataFaker.PassportCredential.Create(ppCredential.Credential, Guid.NewGuid().ToString());

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltReset = await fxtPassport.PassportTokenRepository.ResetCredentialAsync(ppToken.MapToTransferObject(), ppCredentialToApply, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            await rsltReset.MatchAsync(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                async bResult =>
                {
                    RepositoryResult<PassportTokenTransferObject> rsltToken = await fxtPassport.PassportTokenRepository.FindTokenByCredentialAsync(ppCredentialToApply, prvTime.GetUtcNow(), CancellationToken.None);

                    return rsltToken.Match(
                        msgError =>
                        {
                            msgError.Should().BeNull();

                            return false;
                        },
                        dtoPassportTokenInRepository =>
                        {
                            dtoPassportTokenInRepository.PassportId.Should().Be(ppPassport.Id);

                            return true;
                        });
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Reset_ShouldReturnRepositoryError_WhenCredentialDoesNotExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToApply = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltReset = await fxtPassport.PassportTokenRepository.ResetCredentialAsync(ppToken.MapToTransferObject(), ppCredentialToApply, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltReset.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Credential has not been reset at provider {ppCredentialToApply.Provider}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();
                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Reset_ShouldReturnRepositoryError_WhenPassportIdDoesNotExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(Guid.NewGuid());
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppCredentialToApply = DataFaker.PassportCredential.Create(ppCredential.Credential, ppCredential.Signature);

            Guid guPassportId = Guid.NewGuid();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltReset = await fxtPassport.PassportTokenRepository.ResetCredentialAsync(ppToken.MapToTransferObject(), ppCredentialToApply, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltReset.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Credential has not been reset at provider {ppCredentialToApply.Provider}.");

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