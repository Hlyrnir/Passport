using FluentAssertions;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Command.PassportVisa.Create;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.CreatePassportVisa
{
    public sealed class CreatePassportVisaValidationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public CreatePassportVisaValidationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(true, int.MaxValue)]
        public async Task Create_ShouldReturnTrue_WhenLevelIsValid(bool bExpectedResult, int iLevel)
        {
            // Arrange
            CreatePassportVisaCommand cmdCreate = new CreatePassportVisaCommand()
            {
                Level = iLevel,
                Name = Guid.NewGuid().ToString(),
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<CreatePassportVisaCommand> hndlValidation = new CreatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdCreate,
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
        }

        [Theory]
        [InlineData(false, (-1))]
        [InlineData(false, int.MinValue)]
        public async Task Create_ShouldReturnMessageError_WhenLevelIsInvalid(bool bExpectedResult, int iLevel)
        {
            // Arrange
            CreatePassportVisaCommand cmdCreate = new CreatePassportVisaCommand()
            {
                Level = iLevel,
                Name = Guid.NewGuid().ToString(),
                RestrictedPassportId = Guid.NewGuid()
            };

            IValidation<CreatePassportVisaCommand> hndlValidation = new CreatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            // Act
            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdCreate,
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
        }

        [Theory]
        [InlineData(true, "THIS_IS_A_NAME")]
        [InlineData(true, "THIS_IS_A_VERY_VERY_VERY_VERY_VERY_VERY_VERY_VERY_VERY_VERY_LONG_NAME")]
        public async Task Create_ShouldReturnTrue_WhenNameIsValid(bool bExpectedResult, string sName)
        {
            // Arrange
            CreatePassportVisaCommand cmdCreate = new CreatePassportVisaCommand()
            {
                Level = 0,
                Name = sName,
                RestrictedPassportId = Guid.NewGuid(),
            };

            // Act
            IValidation<CreatePassportVisaCommand> hndlValidation = new CreatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdCreate,
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
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        public async Task Create_ShouldReturnMessageError_WhenNameIsInvalid(bool bExpectedResult, string sName)
        {
            // Arrange
            CreatePassportVisaCommand cmdCreate = new CreatePassportVisaCommand()
            {
                Level = 0,
                Name = sName,
                RestrictedPassportId = Guid.NewGuid(),
            };

            // Act
            IValidation<CreatePassportVisaCommand> hndlValidation = new CreatePassportVisaValidation(
                repoVisa: fxtPassport.PassportVisaRepository,
                srvValidation: fxtPassport.PassportValidation,
                prvTime: prvTime);

            IMessageResult<bool> rsltValidation = await hndlValidation.ValidateAsync(
                msgMessage: cmdCreate,
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
        }
    }
}