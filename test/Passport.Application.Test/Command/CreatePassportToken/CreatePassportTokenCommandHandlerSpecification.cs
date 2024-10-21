using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.Create;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.CreatePassportToken
{
    public sealed class CreatePassportTokenCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public CreatePassportTokenCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Create_ShouldReturnTrue_WhenTokenIsCreated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppCredentialToAdd = DataFaker.PassportCredential.CreateAtProvider(sProvider: "DEFAULT_UNDEFINED");

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredentialToVerify, prvTime.GetUtcNow(), CancellationToken.None);

            CreatePassportTokenCommand cmdCreate = new CreatePassportTokenCommand()
            {
                CredentialToVerify = ppCredentialToVerify,
                CredentialToAdd = ppCredentialToAdd,
                RestrictedPassportId = Guid.NewGuid()
            };

            CreatePassportTokenCommandHandler cmdHandler = new CreatePassportTokenCommandHandler(
                prvTime: prvTime,
                ppSetting: fxtPassport.PassportSetting,
                repoToken: fxtPassport.PassportTokenRepository);

            // Act
            IMessageResult<Guid> rsltTokenId = await cmdHandler.Handle(cmdCreate, CancellationToken.None);

            // Assert
            await rsltTokenId.MatchAsync(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                async guPassportTokenId =>
                {
                    RepositoryResult<PassportTokenTransferObject> rsltToken = await fxtPassport.PassportTokenRepository.FindTokenByCredentialAsync(ppCredentialToAdd, prvTime.GetUtcNow(), CancellationToken.None);

                    return rsltToken.Match(
                        msgError =>
                        {
                            msgError.Should().BeNull();

                            return false;
                        },
                        dtoTokenInRepository =>
                        {
                            dtoTokenInRepository.Id.Should().Be(guPassportTokenId);

                            return true;
                        });
                });

            //Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Create_ShouldReturnRepositoryError_WhenTokenIsNotCreated()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredentialToVerify = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppCredentialToAdd = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredentialToVerify, prvTime.GetUtcNow(), CancellationToken.None);

            CreatePassportTokenCommand cmdCreate = new CreatePassportTokenCommand()
            {
                CredentialToVerify = ppCredentialToVerify,
                CredentialToAdd = ppCredentialToAdd,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            CreatePassportTokenCommandHandler cmdHandler = new CreatePassportTokenCommandHandler(
                prvTime: prvTime,
                ppSetting: fxtPassport.PassportSetting,
                repoToken: fxtPassport.PassportTokenRepository);

            IMessageResult<Guid> rsltTokenId = await cmdHandler.Handle(cmdCreate, CancellationToken.None);

            // Assert
            rsltTokenId.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DomainError.Code.Method);
                    msgError.Description.Should().Be($"Token at provider {cmdCreate.CredentialToAdd.Provider} does already exist.");
                    return false;
                },
                guPassportVisaId =>
                {
                    guPassportVisaId.Should().BeEmpty();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}