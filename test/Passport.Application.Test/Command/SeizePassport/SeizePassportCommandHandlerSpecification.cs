using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Seize;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.SeizePassport
{
    public sealed class SeizePassportCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public SeizePassportCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenVisaIsDeleted()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            SeizePassportCommand cmdDelete = new SeizePassportCommand()
            {
                PassportIdToSeize = ppPassport.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            SeizePassportCommandHandler cmdHandler = new SeizePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository);

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

            SeizePassportCommand cmdDelete = new SeizePassportCommand()
            {
                PassportIdToSeize = Guid.Empty,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            // Act
            SeizePassportCommandHandler cmdHandler = new SeizePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository);

            IMessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.Passport.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.Passport.NotFound.Description);
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
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            SeizePassportCommand cmdDelete = new SeizePassportCommand()
            {
                PassportIdToSeize = Guid.Empty,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            // Act
            SeizePassportCommandHandler cmdHandler = new SeizePassportCommandHandler(
                prvTime: prvTime,
                repoPassport: fxtPassport.PassportRepository);

            IMessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.Passport.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.Passport.NotFound.Description);
                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }
    }
}