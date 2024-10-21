using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Register;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.RegisterPassport
{
    public class RegisterPassportCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public RegisterPassportCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Register_ShouldReturnTrue_WhenPassportIsCreated()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            RegisterPassportCommand cmdRegister = new RegisterPassportCommand()
            {
                CredentialToRegister = ppCredential,
                CultureName = "en-GB",
                EmailAddress = "default@ema.il",
                FirstName = "Jane",
                LastName = "Doe",
                IssuedBy = Guid.NewGuid(),
                PhoneNumber = "111",
                RestrictedPassportId = Guid.NewGuid()
            };

            RegisterPassportCommandHandler hdlCommand = new RegisterPassportCommandHandler(
                prvTime: prvTime,
                uowUnitOfWork: fxtPassport.UnitOfWork,
                ppSetting: fxtPassport.PassportSetting,
                repoPassport: fxtPassport.PassportRepository,
                repoHolder: fxtPassport.PassportHolderRepository,
                repoToken: fxtPassport.PassportTokenRepository);

            // Act
            IMessageResult<Guid> rsltPassportId = await hdlCommand.Handle(cmdRegister, CancellationToken.None);

            Guid guPassportId = Guid.Empty;
            Guid guPassportHolderId = Guid.Empty;
            Guid guPassportTokenId = Guid.Empty;

            //Assert
            await rsltPassportId.MatchAsync(
                msgError =>
                {
                    msgError.Should().BeNull();
                    return false;
                },
                async guPassportIdInRepository =>
                {
                    guPassportId = guPassportIdInRepository;

                    RepositoryResult<PassportTransferObject> rsltPassport = await fxtPassport.PassportRepository.FindByIdAsync(guPassportIdInRepository, CancellationToken.None);

                    return await rsltPassport.MatchAsync(
                        msgError =>
                        {
                            msgError.Should().BeNull();
                            return false;
                        },
                        async dtoPassportInRepository =>
                        {
                            dtoPassportInRepository.Id.Should().Be(guPassportIdInRepository);
                            dtoPassportInRepository.ExpiredAt.Should().Be(prvTime.GetUtcNow().Add(fxtPassport.PassportSetting.PassportExpiresAfterDuration));
                            dtoPassportInRepository.IsAuthority.Should().BeFalse();
                            dtoPassportInRepository.IsEnabled.Should().BeFalse();
                            dtoPassportInRepository.IssuedBy.Should().Be(cmdRegister.IssuedBy);
                            dtoPassportInRepository.LastCheckedAt.Should().Be(prvTime.GetUtcNow());
                            dtoPassportInRepository.LastCheckedBy.Should().Be(cmdRegister.IssuedBy);
                            dtoPassportInRepository.VisaId.Should().BeEmpty();

                            RepositoryResult<PassportHolderTransferObject> rsltHolder = await fxtPassport.PassportHolderRepository.FindByIdAsync(dtoPassportInRepository.HolderId, CancellationToken.None);

                            rsltHolder.Match(
                                msgError =>
                                {
                                    return false;
                                },
                                dtoHolder =>
                                {
                                    guPassportHolderId = dtoHolder.Id;

                                    dtoHolder.CultureName.Should().Be(cmdRegister.CultureName);
                                    dtoHolder.EmailAddress.Should().Be(cmdRegister.EmailAddress);
                                    dtoHolder.EmailAddressIsConfirmed.Should().BeFalse();
                                    dtoHolder.FirstName.Should().Be(cmdRegister.FirstName);
                                    dtoHolder.LastName.Should().Be(cmdRegister.LastName);
                                    dtoHolder.PhoneNumber.Should().Be(cmdRegister.PhoneNumber);
                                    dtoHolder.PhoneNumberIsConfirmed.Should().BeFalse();

                                    return true;
                                });

                            RepositoryResult<PassportTokenTransferObject> rsltToken = await fxtPassport.PassportTokenRepository.FindTokenByCredentialAsync(ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

                            rsltToken.Match(
                                msgError =>
                                {
                                    return false;
                                },
                                dtoToken =>
                                {
                                    guPassportTokenId = dtoToken.Id;

                                    dtoToken.PassportId.Should().Be(guPassportId);
                                    dtoToken.Provider.Should().Be(ppCredential.Provider);
                                    dtoToken.TwoFactorIsEnabled.Should().BeFalse();
                                    return true;
                                });

                            return true;
                        });
                });

            //Clean up
            RepositoryResult<PassportTransferObject> rsltPassportToDelete = await fxtPassport.PassportRepository.FindByIdAsync(guPassportId, CancellationToken.None);

            await rsltPassportToDelete.MatchAsync(
                msgError => false,
                async dtoPassport => await fxtPassport.PassportRepository.DeleteAsync(dtoPassport, CancellationToken.None));

            RepositoryResult<PassportHolderTransferObject> rsltHolderToDelete = await fxtPassport.PassportHolderRepository.FindByIdAsync(guPassportHolderId, CancellationToken.None);

            await rsltHolderToDelete.MatchAsync(
                msgError => false,
                async dtoHolder => await fxtPassport.PassportHolderRepository.DeleteAsync(dtoHolder, CancellationToken.None));

            RepositoryResult<PassportTokenTransferObject> rsltTokenToDelete = await fxtPassport.PassportTokenRepository.FindTokenByCredentialAsync(ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            await rsltTokenToDelete.MatchAsync(
                msgError => false,
                async dtoToken => await fxtPassport.PassportTokenRepository.DeleteAsync(dtoToken, CancellationToken.None));
        }
    }
}
