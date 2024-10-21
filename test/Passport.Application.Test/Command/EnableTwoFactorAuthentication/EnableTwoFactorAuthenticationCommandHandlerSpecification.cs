using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.EnableTwoFactorAuthentication
{
    public class EnableTwoFactorAuthenticationCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public EnableTwoFactorAuthenticationCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenTwoFactorIsEnabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            EnableTwoFactorAuthenticationCommand cmdUpdate = new EnableTwoFactorAuthenticationCommand()
            {
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid(),
                TwoFactorIsEnabled = true
            };

            EnableTwoFactorAuthenticationCommandHandler hdlCommand = new EnableTwoFactorAuthenticationCommandHandler(
                prvTime: prvTime,
                repoToken: fxtPassport.PassportTokenRepository);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            await rsltUpdate.MatchAsync(
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
                        dtoTokenInRepository =>
                        {
                            dtoTokenInRepository.Id.Should().Be(ppToken.Id);
                            dtoTokenInRepository.PassportId.Should().Be(ppToken.PassportId);
                            dtoTokenInRepository.Provider.Should().Be(ppToken.Provider);
                            dtoTokenInRepository.RefreshToken.Should().NotBe(ppToken.RefreshToken);
                            dtoTokenInRepository.TwoFactorIsEnabled.Should().BeTrue();

                            return true;
                        });
                });

            //Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenTwoFactorIsEnabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            ppToken.TryEnableTwoFactorAuthentication(true);

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            EnableTwoFactorAuthenticationCommand cmdUpdate = new EnableTwoFactorAuthenticationCommand()
            {
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid(),
                TwoFactorIsEnabled = true
            };

            EnableTwoFactorAuthenticationCommandHandler hdlCommand = new EnableTwoFactorAuthenticationCommandHandler(
                prvTime: prvTime,
                repoToken: fxtPassport.PassportTokenRepository);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DomainError.Code.Method);
                    msgError.Description.Should().Be("Two factor authentication is already enabled.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            //Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}