using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Query.PassportVisa.ByPassportId;

namespace Passport.Application.Test.Query.PassportVisaByPassportId
{
    public sealed class PassportVisaByPassportIdValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportVisaByPassportIdValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldReturnTrue_WhenPassportIdExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportVisaByPassportIdQuery qryById = new PassportVisaByPassportIdQuery()
            {
                PassportIdToFind = ppPassport.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportVisaByPassportIdQuery> hndlValidation = new PassportVisaByPassportIdValidation(
                repoPassport: fxtPassport.PassportRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: fxtPassport.TimeProvider);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: qryById,
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
        public async Task Find_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            Guid guPassportId = Guid.NewGuid();

            PassportVisaByPassportIdQuery qryById = new PassportVisaByPassportIdQuery()
            {
                PassportIdToFind = guPassportId,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportVisaByPassportIdQuery> hndlValidation = new PassportVisaByPassportIdValidation(
                repoPassport: fxtPassport.PassportRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: fxtPassport.TimeProvider);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: qryById,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Passport {guPassportId} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }

        [Fact]
        public async Task Read_ShouldReturnMessageError_WhenPassportIdIsEmpty()
        {
            // Arrange
            PassportVisaByPassportIdQuery qryById = new PassportVisaByPassportIdQuery()
            {
                PassportIdToFind = Guid.Empty,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportVisaByPassportIdQuery> hndlValidation = new PassportVisaByPassportIdValidation(
                repoPassport: fxtPassport.PassportRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: fxtPassport.TimeProvider);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: qryById,
                tknCancellation: CancellationToken.None);

            // Assert
            rsltValidation.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(ValidationError.Code.Method);
                    msgError.Description.Should().Contain($"Passport identifier is invalid (empty).");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }
    }
}