using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Application.Command.PassportVisa.Update;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Application.Test.Command.UpdatePassportVisa
{
    public sealed class UpdatePassportVisaCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportVisaCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenVisaIsUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = 0,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            UpdatePassportVisaCommandHandler cmdHandler = new UpdatePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<bool> rsltUpdate = await cmdHandler.Handle(cmdUpdate, CancellationToken.None);

            // Assert
            rsltUpdate.Match(
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
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnRepositoryError_WhenVisaDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Level = 0,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = Guid.Empty,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            UpdatePassportVisaCommandHandler cmdHandler = new UpdatePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            MessageResult<bool> rsltUpdate = await cmdHandler.Handle(cmdUpdate, CancellationToken.None);

            // Assert
            rsltUpdate.Match(
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
        public async Task Update_ShouldReturnRepositoryError_WhenConcurrencyStampDoNotMatch()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            string sObsoleteConcurrencyStamp = Guid.NewGuid().ToString();

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = sObsoleteConcurrencyStamp,
                Level = 0,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            UpdatePassportVisaCommandHandler cmdHandler = new UpdatePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<bool> rsltUpdate = await cmdHandler.Handle(cmdUpdate, CancellationToken.None);

            // Assert
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

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnRepositoryError_WhenVisaIsNotUpdated()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = -1,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            UpdatePassportVisaCommandHandler cmdHandler = new UpdatePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            MessageResult<bool> rsltUpdate = await cmdHandler.Handle(cmdUpdate, CancellationToken.None);

            // Assert
            rsltUpdate.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DomainError.Code.Method);
                    msgError.Description.Should().Be("Level could not be changed.");

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