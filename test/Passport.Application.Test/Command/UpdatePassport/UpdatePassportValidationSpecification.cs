using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.Passport.Update;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.UpdatePassport
{
    public sealed class UpdatePassportValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = ppPassport.ExpiredAt,
                IsAuthority = false,
                IsEnabled = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportCommand> hndlValidation = new UpdatePassportValidation(
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
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
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                ExpiredAt = prvTime.GetUtcNow(),
                IsAuthority = false,
                IsEnabled = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = Guid.NewGuid(),
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportCommand> hndlValidation = new UpdatePassportValidation(
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Passport {cmdUpdate.PassportIdToUpdate} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenExpiredAtDateIsLater()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DateTimeOffset dtDate = prvTime.GetUtcNow().AddDays(1);

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = dtDate,
                IsAuthority = false,
                IsEnabled = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportCommand> hndlValidation = new UpdatePassportValidation(
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            //Assert
            rsltValidation.Match(
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
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenExpiredAtIsTooEarly()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            DateTimeOffset dtDate = prvTime.GetUtcNow().AddDays(-1);

            UpdatePassportCommand cmdUpdate = new UpdatePassportCommand()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = dtDate,
                IsAuthority = false,
                IsEnabled = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = ppPassport.Id,
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportCommand> hndlValidation = new UpdatePassportValidation(
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            //Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();

                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain("Expiration date must be in the future.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}