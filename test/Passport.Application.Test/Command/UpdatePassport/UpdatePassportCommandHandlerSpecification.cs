using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Update;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.UpdatePassport
{
    public class UpdatePassportCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIsUpdated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow());
            ppPassport.TryJoinToAuthority(ppAuthority, prvTime.GetUtcNow());
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = ppPassport.ExpiredAt.AddDays(1),
                IsAuthority = ppPassport.IsAuthority,
                IsEnabled = ppPassport.IsEnabled,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = ppPassport.VisaId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            UpdatePassportCommandHandler hdlCommand = new UpdatePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

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

                    RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(ppPassport.Id, CancellationToken.None);

                    return rsltPassport.Match(
                        msgError => false,
                        dtoPassportInRepository =>
                        {
                            dtoPassportInRepository.ExpiredAt.Should().Be(ppPassport.ExpiredAt.AddDays(1));
                            dtoPassportInRepository.IsAuthority.Should().Be(true);
                            dtoPassportInRepository.IsEnabled.Should().Be(true);
                            dtoPassportInRepository.LastCheckedAt.Should().Be(prvTime.GetUtcNow());
                            dtoPassportInRepository.LastCheckedBy.Should().Be(ppPassport.Id);
                            dtoPassportInRepository.VisaId.Should().BeEmpty();

                            return true;
                        });
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnRepositoryError_WhenConcurrencyDoNotMatch()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow());
            ppPassport.TryJoinToAuthority(ppAuthority, prvTime.GetUtcNow());
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            string sObsoleteConcurrencyStamp = Guid.NewGuid().ToString();

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = sObsoleteConcurrencyStamp,
                ExpiredAt = ppPassport.ExpiredAt.AddDays(1),
                IsAuthority = ppPassport.IsAuthority,
                IsEnabled = ppPassport.IsEnabled,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = ppPassport.VisaId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            UpdatePassportCommandHandler hdlCommand = new UpdatePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(DefaultMessageError.ConcurrencyViolation);

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenTryToEnablePassportWithoutAuthorization()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = ppPassport.ExpiredAt.AddDays(1),
                IsAuthority = ppPassport.IsAuthority,
                IsEnabled = true,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = ppPassport.VisaId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            UpdatePassportCommandHandler hdlCommand = new UpdatePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();

                    msgError.Code.Should().Be(DomainError.Code.Method);
                    msgError.Description.Should().Be($"Passport {ppPassport.Id} is not enabled.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenTryToJoinAuthorityWithoutAuthorization()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow());
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = ppPassport.ExpiredAt.AddDays(1),
                IsAuthority = true,
                IsEnabled = ppPassport.IsEnabled,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = ppPassport.VisaId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            UpdatePassportCommandHandler hdlCommand = new UpdatePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            IMessageResult<bool> rsltUpdate = await hdlCommand.Handle(cmdUpdate, CancellationToken.None);

            //Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();

                    msgError.Code.Should().Be(DomainError.Code.Method);
                    msgError.Description.Should().Be($"Passport {ppPassport.Id} could not join to authority.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}