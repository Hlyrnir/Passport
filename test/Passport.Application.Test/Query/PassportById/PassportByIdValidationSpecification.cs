using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Query.Passport.ById;

namespace Passport.Application.Test.Query.PassportById
{
    public sealed class PassportByIdValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportByIdValidationSpecification(PassportFixture fxtPassport)
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

            PassportByIdQuery qryById = new PassportByIdQuery()
            {
                PassportId = ppPassport.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportByIdQuery> hndlValidation = new PassportByIdValidation(
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

            PassportByIdQuery qryById = new PassportByIdQuery()
            {
                PassportId = guPassportId,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportByIdQuery> hndlValidation = new PassportByIdValidation(
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
        public async Task Find_ShouldReturnMessageError_WhenPassportIdIsEmpty()
        {
            // Arrange
            PassportByIdQuery qryById = new PassportByIdQuery()
            {
                PassportId = Guid.Empty,
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<PassportByIdQuery> hndlValidation = new PassportByIdValidation(
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