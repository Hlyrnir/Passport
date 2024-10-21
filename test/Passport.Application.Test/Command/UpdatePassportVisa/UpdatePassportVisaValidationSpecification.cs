using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportVisa.Update;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.UpdatePassportVisa
{
    public sealed class UpdatePassportVisaValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportVisaValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportVisaExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdDelete = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = 0,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<UpdatePassportVisaCommand> hndlValidation = new UpdatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdDelete,
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
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenPassportVisaDoesNotExist()
        {
            // Arrange
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            UpdatePassportVisaCommand cmdDelete = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Level = 0,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = Guid.NewGuid(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.Empty
            };

            IValidation<UpdatePassportVisaCommand> hndlValidation = new UpdatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdDelete,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Passport visa {cmdDelete.PassportVisaId} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(true, int.MaxValue)]
        public async Task Update_ShouldReturnTrue_WhenLevelIsValid(bool bExpectedResult, int iLevel)
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = iLevel,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportVisaCommand> hndlValidation = new UpdatePassportVisaValidation(
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
                    bResult.Should().Be(bExpectedResult);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Theory]
        [InlineData(false, (-1))]
        [InlineData(false, int.MinValue)]
        public async Task Update_ShouldReturnMessageError_WhenLevelIsInvalid(bool bExpectedResult, int iLevel)
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = iLevel,
                Name = Guid.NewGuid().ToString(),
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<UpdatePassportVisaCommand> hndlValidation = new UpdatePassportVisaValidation(
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
                    msgError.Description.Should().Contain("Level must be greater than or equal to zero.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().Be(bExpectedResult);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Theory]
        [InlineData(true, "THIS_IS_A_NAME")]
        [InlineData(true, "THIS_IS_A_VERY_VERY_VERY_VERY_VERY_VERY_VERY_VERY_VERY_VERY_LONG_NAME")]
        public async Task Update_ShouldReturnTrue_WhenNameIsValid(bool bExpectedResult, string sName)
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = 0,
                Name = sName,
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            IValidation<UpdatePassportVisaCommand> hndlValidation = new UpdatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

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
                    bResult.Should().Be(bExpectedResult);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        public async Task Update_ShouldReturnMessageError_WhenNameIsInvalid(bool bExpectedResult, string sName)
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportVisaCommand cmdUpdate = new UpdatePassportVisaCommand()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Level = 0,
                Name = sName,
                PassportVisaId = ppVisa.Id,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            IValidation<UpdatePassportVisaCommand> hndlValidation = new UpdatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdUpdate,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain("Name is invalid (empty).");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().Be(bExpectedResult);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }
    }
}