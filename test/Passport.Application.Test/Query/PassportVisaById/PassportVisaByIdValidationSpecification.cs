using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Query.PassportVisa.ById;

namespace Passport.Application.Test.Query.PassportVisaById
{
    public sealed class PassportVisaByIdValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportVisaByIdValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldReturnTrue_WhenPassportVisaIdExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportVisaByIdQuery qryById = new PassportVisaByIdQuery()
            {
                PassportVisaId = ppVisa.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportVisaByIdQuery> hndlValidation = new PassportVisaByIdValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
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
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Find_ShouldReturnMessageError_WhenPassportVisaDoesNotExist()
        {
            // Arrange
            Guid guPassportVisaId = Guid.NewGuid();

            PassportVisaByIdQuery qryById = new PassportVisaByIdQuery()
            {
                PassportVisaId = guPassportVisaId,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportVisaByIdQuery> hndlValidation = new PassportVisaByIdValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
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
                    msgError.Description.Should().Contain($"Passport visa {guPassportVisaId} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }

        [Fact]
        public async Task Find_ShouldReturnMessageError_WhenPassportVisaIdIsEmpty()
        {
            // Arrange
            PassportVisaByIdQuery qryById = new PassportVisaByIdQuery()
            {
                PassportVisaId = Guid.Empty,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportVisaByIdQuery> hndlValidation = new PassportVisaByIdValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
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
                    msgError.Description.Should().Contain($"Passport visa identifier is invalid (empty).");

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