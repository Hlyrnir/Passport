using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Authentication.ByCredential;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.AuthenticationTokenByCredential
{
    public sealed class AuthenticationTokenByCredentialCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public AuthenticationTokenByCredentialCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task ByCredential_ShouldReturnToken_WhenPassportIsEnabled()
        {
            // Arrange
            IAuthenticationTokenHandler<Guid> authHandler = fxtPassport.AuthenticationHandler;

            Domain.Aggregate.Passport ppAutority = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            ppPassport.TryEnable(ppAutority, prvTime.GetUtcNow());
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);

            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppCredential
            };

            AuthenticationTokenByCredentialCommandHandler cmdHandler = new AuthenticationTokenByCredentialCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoToken: fxtPassport.PassportTokenRepository,
                authTokenHandler: authHandler);

            // Act
            IMessageResult<AuthenticationTokenTransferObject> rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);

            // Assert
            rsltJwtToken.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoAuthenticationToken =>
                {
                    dtoAuthenticationToken.ExpiredAt.Should().Be(ppPassport.ExpiredAt);
                    dtoAuthenticationToken.Provider.Should().Be(ppToken.Provider);
                    dtoAuthenticationToken.RefreshToken.Should().NotBe(ppToken.RefreshToken);
                    dtoAuthenticationToken.Token.Should().Be(authHandler.Generate(ppPassport.Id, prvTime));

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task ByCredential_ShouldReturnMessageError_WhenPassportIsDisabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppAutority = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            ppPassport.TryDisable(ppAutority, ppPassport.ExpiredAt.AddDays(-1));
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppCredential
            };

            // Act
            AuthenticationTokenByCredentialCommandHandler cmdHandler = new AuthenticationTokenByCredentialCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoToken: fxtPassport.PassportTokenRepository,
                authTokenHandler: fxtPassport.AuthenticationHandler);

            IMessageResult<AuthenticationTokenTransferObject> rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);

            // Assert
            rsltJwtToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().BeEquivalentTo(AuthorizationError.Passport.IsDisabled);

                    return false;
                },
                dtoJwtToken =>
                {
                    dtoJwtToken.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task ByCredential_ShouldReturnMessageError_WhenPassportIsExpired()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            FakeTimeProvider prvFakeTime = new FakeTimeProvider();
            prvFakeTime.SetUtcNow(ppPassport.ExpiredAt.AddDays(1));

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppCredential
            };

            // Act
            AuthenticationTokenByCredentialCommandHandler cmdHandler = new AuthenticationTokenByCredentialCommandHandler(
                prvTime: prvFakeTime,
                repoPassport: fxtPassport.PassportRepository,
                repoToken: fxtPassport.PassportTokenRepository,
                authTokenHandler: fxtPassport.AuthenticationHandler);

            IMessageResult<AuthenticationTokenTransferObject> rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);

            // Assert
            rsltJwtToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().BeEquivalentTo(AuthorizationError.Passport.IsExpired);

                    return false;
                },
                dtoJwtToken =>
                {
                    dtoJwtToken.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task ByCredential_ShouldReturnMessageError_WhenCredentialIsUnknown()
        {
            // Arrange
            Domain.Aggregate.Passport ppAutority = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            ppPassport.TryEnable(ppAutority, prvTime.GetUtcNow());
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            IPassportCredential ppInvalidCredential = DataFaker.PassportCredential.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppInvalidCredential
            };

            // Act
            AuthenticationTokenByCredentialCommandHandler cmdHandler = new AuthenticationTokenByCredentialCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoToken: fxtPassport.PassportTokenRepository,
                authTokenHandler: fxtPassport.AuthenticationHandler);

            IMessageResult<AuthenticationTokenTransferObject> rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);

            // Assert
            rsltJwtToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportToken.Credential.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportToken.Credential.NotFound.Description);

                    return false;
                },
                dtoJwtToken =>
                {
                    dtoJwtToken.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task ByCredential_ShouldReturnMessageError_WhenSignatureIsInvalid()
        {
            // Arrange
            Domain.Aggregate.Passport ppAutority = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            ppPassport.TryEnable(ppAutority, prvTime.GetUtcNow());
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            IPassportCredential ppInvalidCredential = DataFaker.PassportCredential.Create(ppCredential.Credential, Guid.NewGuid().ToString());

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppInvalidCredential
            };

            // Act
            AuthenticationTokenByCredentialCommandHandler cmdHandler = new AuthenticationTokenByCredentialCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoToken: fxtPassport.PassportTokenRepository,
                authTokenHandler: fxtPassport.AuthenticationHandler);

            IMessageResult<AuthenticationTokenTransferObject> rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);

            // Assert
            rsltJwtToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportToken.Credential.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportToken.Credential.NotFound.Description);

                    return false;
                },
                dtoJwtToken =>
                {
                    dtoJwtToken.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task ByCredential_ShouldReturnMessageError_WhenTooManyAttemps()
        {
            // Arrange
            Domain.Aggregate.Passport ppAutority = DataFaker.Passport.CreateAuthority();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            ppPassport.TryEnable(ppAutority, prvTime.GetUtcNow());
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

            IPassportCredential ppInvalidCredential = DataFaker.PassportCredential.Create(ppCredential.Credential, Guid.NewGuid().ToString());

            AuthenticationTokenByCredentialCommand cmdToken = new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppInvalidCredential
            };

            // Act
            AuthenticationTokenByCredentialCommandHandler cmdHandler = new AuthenticationTokenByCredentialCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository,
                repoToken: fxtPassport.PassportTokenRepository,
                authTokenHandler: fxtPassport.AuthenticationHandler);

            IMessageResult<AuthenticationTokenTransferObject> rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);

            for (int i = 0; i < fxtPassport.PassportSetting.MaximalAllowedAccessAttempt; i++)
            {
                rsltJwtToken = await cmdHandler.Handle(cmdToken, CancellationToken.None);
            }

            // Assert
            rsltJwtToken.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().BeEquivalentTo(AuthenticationError.TooManyAttempts);

                    return false;
                },
                dtoJwtToken =>
                {
                    dtoJwtToken.Should().BeNull();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}