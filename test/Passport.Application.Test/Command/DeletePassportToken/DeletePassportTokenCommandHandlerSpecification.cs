using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.Delete;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.DeletePassportToken
{
    public sealed class DeletePassportTokenCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportTokenCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenTokenIsDeleted()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppInitialToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            Domain.Aggregate.PassportToken ppAdditionalToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppInitialCredential = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppAdditionalCredential = DataFaker.PassportCredential.Create(ppInitialCredential.Credential, "another_$ignatUr3");

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppInitialToken.MapToTransferObject(), ppInitialCredential, prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppAdditionalToken.MapToTransferObject(), ppAdditionalCredential, prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportTokenCommand cmdDelete = new DeletePassportTokenCommand()
            {
                CredentialToRemove = ppAdditionalCredential,
                CredentialToVerify = ppInitialCredential,
                RestrictedPassportId = Guid.Empty
            };

            DeletePassportTokenCommandHandler cmdHandler = new DeletePassportTokenCommandHandler(
                prvTime: prvTime,
                repoToken: fxtPassport.PassportTokenRepository);

            // Act
            IMessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
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
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppInitialToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Delete_ShouldReturnRepositoryError_WhenCredentialDoesNotExist()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
            IPassportCredential ppInitialCredential = DataFaker.PassportCredential.CreateDefault();
            IPassportCredential ppAdditionalCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppInitialCredential, prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportTokenCommand cmdDelete = new DeletePassportTokenCommand()
            {
                CredentialToRemove = ppAdditionalCredential,
                CredentialToVerify = ppInitialCredential,
                RestrictedPassportId = Guid.Empty
            };

            // Act
            DeletePassportTokenCommandHandler cmdHandler = new DeletePassportTokenCommandHandler(
                prvTime: prvTime,
                repoToken: fxtPassport.PassportTokenRepository);

            IMessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportToken.Code.Method);
                    msgError.Description.Should().Be(TestError.Repository.PassportToken.Credential.NotFound.Description);
                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });

            // Clean up
            await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}