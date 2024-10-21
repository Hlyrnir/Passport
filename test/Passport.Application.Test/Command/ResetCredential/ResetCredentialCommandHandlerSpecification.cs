using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.ResetCredential;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Application.Test.Command.ResetCredential
{
    public class ResetCredentialCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public ResetCredentialCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportCredentialIsReset()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredentialToVerify, prvTime.GetUtcNow(), CancellationToken.None);

            IPassportCredential ppCredentialToApply = DataFaker.PassportCredential.CreateDefault();

            ResetCredentialCommand cmdUpdate = new ResetCredentialCommand()
            {
                CredentialToApply = ppCredentialToApply,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = Guid.NewGuid()
            };

            ResetCredentialCommandHandler hdlCommand = new ResetCredentialCommandHandler(
                prvTime: prvTime,
                uowUnitOfWork: fxtPassport.UnitOfWork,
                ppSetting: fxtPassport.PassportSetting,
                repoPassport: fxtPassport.PassportRepository,
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

                    RepositoryResult<int> rsltToken = await fxtPassport.PassportTokenRepository.VerifyCredentialAsync(ppCredentialToApply, prvTime.GetUtcNow(), CancellationToken.None);

                    return rsltToken.Match(
                        msgError => false,
                        iNumberOfRemainingAttempts =>
                        {
                            iNumberOfRemainingAttempts.Should().Be(fxtPassport.PassportSetting.MaximalAllowedAccessAttempt);

                            return true;
                        });
                });

            //Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}