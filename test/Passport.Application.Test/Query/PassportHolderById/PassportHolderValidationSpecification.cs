using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Query.PassportHolder.ById;

namespace Passport.Application.Test.Query.PassportHolderById
{
    public sealed class PassportHolderByIdValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportHolderByIdValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldReturnTrue_WhenPassportHolderIdExists()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault(fxtPassport.PassportSetting);

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportHolderByIdQuery qryById = new PassportHolderByIdQuery()
            {
                PassportHolderId = ppHolder.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportHolderByIdQuery> hndlValidation = new PassportHolderByIdValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
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
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Find_ShouldReturnMessageError_WhenPassportHolderDoesNotExist()
        {
            // Arrange
            Guid guPassportHolderId = Guid.NewGuid();

            PassportHolderByIdQuery qryById = new PassportHolderByIdQuery()
            {
                PassportHolderId = guPassportHolderId,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportHolderByIdQuery> hndlValidation = new PassportHolderByIdValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
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
                    msgError.Description.Should().Contain($"Passport holder {guPassportHolderId} does not exist.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });
        }

        [Fact]
        public async Task Find_ShouldReturnMessageError_WhenPassportHolderIdIsEmpty()
        {
            // Arrange
            PassportHolderByIdQuery qryById = new PassportHolderByIdQuery()
            {
                PassportHolderId = Guid.Empty,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportHolderByIdQuery> hndlValidation = new PassportHolderByIdValidation(
                repoHolder: fxtPassport.PassportHolderRepository,
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
                    msgError.Description.Should().Contain($"Passport holder identifier is invalid (empty).");

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