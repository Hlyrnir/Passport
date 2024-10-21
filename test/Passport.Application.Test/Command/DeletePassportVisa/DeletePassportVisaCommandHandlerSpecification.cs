using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Command.PassportVisa.Delete;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Application.Test.Command.DeletePassportVisa
{
    public sealed class DeletePassportVisaCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public DeletePassportVisaCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenVisaIsDeleted()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportVisaCommand cmdDelete = new DeletePassportVisaCommand()
            {
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            DeletePassportVisaCommandHandler cmdHandler = new DeletePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

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
        public async Task Delete_ShouldReturnRepositoryError_WhenVisaDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            DeletePassportVisaCommand cmdDelete = new DeletePassportVisaCommand()
            {
                PassportVisaId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            DeletePassportVisaCommandHandler cmdHandler = new DeletePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            MessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportVisa.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportVisa.NotFound.Description);
                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return false;
                });
        }

        [Fact]
        public async Task Delete_ShouldReturnRepositoryError_WhenVisaIsNotDeleted()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DeletePassportVisaCommand cmdDelete = new DeletePassportVisaCommand()
            {
                PassportVisaId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            DeletePassportVisaCommandHandler cmdHandler = new DeletePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            MessageResult<bool> rsltDelete = await cmdHandler.Handle(cmdDelete, CancellationToken.None);

            // Assert
            rsltDelete.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportVisa.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportVisa.NotFound.Description);
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