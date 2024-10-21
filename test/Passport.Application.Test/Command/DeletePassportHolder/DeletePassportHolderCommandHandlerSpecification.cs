using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.Delete;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.DeletePassportHolder
{
    public sealed class DeletePassportHolderCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportHolderCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenHolderIsDeleted()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtPassport.PassportSetting);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportHolderCommand cmdDelete = new DeletePassportHolderCommand()
            {
                PassportHolderId = ppHolder.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            DeletePassportHolderCommandHandler cmdHandler = new DeletePassportHolderCommandHandler(
                prvTime: prvTime,
                repoHolder: fxtPassport.PassportHolderRepository);

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
        }

        [Fact]
        public async Task Delete_ShouldReturnRepositoryError_WhenHolderDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            DeletePassportHolderCommand cmdDelete = new DeletePassportHolderCommand()
            {
                PassportHolderId = Guid.Empty,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            // Act
            DeletePassportHolderCommandHandler cmdHandler = new DeletePassportHolderCommandHandler(
                prvTime: prvTime,
                repoHolder: fxtPassport.PassportHolderRepository);

            IMessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportHolder.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportHolder.NotFound.Description);
                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });
        }

        [Fact]
        public async Task Delete_ShouldReturnRepositoryError_WhenHolderIsNotDeleted()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtPassport.PassportSetting);
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportHolderCommand cmdDelete = new DeletePassportHolderCommand()
            {
                PassportHolderId = Guid.Empty,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            // Act
            DeletePassportHolderCommandHandler cmdHandler = new DeletePassportHolderCommandHandler(
                prvTime: prvTime,
                repoHolder: fxtPassport.PassportHolderRepository);

            IMessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportHolder.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportHolder.NotFound.Description);
                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });

            // Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }
    }
}
