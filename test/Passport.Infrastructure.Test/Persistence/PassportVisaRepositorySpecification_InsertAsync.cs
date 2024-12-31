using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_InsertAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_InsertAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnTrue_WhenVisaIsCreated()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            // Act
            RepositoryResult<bool> rsltVisa = await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltVisa.Match(
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
        public async Task InsertAsync_ShouldReturnRepositoryError_WhenPassportVisaIdExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            PassportVisaTransferObject dtoVisa = ppVisa.MapToTransferObject();
            PassportVisaTransferObject dtoVisaToCreate = new PassportVisaTransferObject()
            {
                ConcurrencyStamp = dtoVisa.ConcurrencyStamp,
                Id = dtoVisa.Id,
                Level = 1,
                Name = "ANOTHER_DEFAULT"
            };

            await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisa, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisaToCreate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not create visa {dtoVisaToCreate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisa, CancellationToken.None);
        }

        [Fact]
        public async Task InsertAsync_ShouldReturnRepositoryError_WhenNameAndLevelExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            PassportVisaTransferObject dtoVisa = ppVisa.MapToTransferObject();
            PassportVisaTransferObject dtoVisaToCreate = new PassportVisaTransferObject()
            {
                ConcurrencyStamp = dtoVisa.ConcurrencyStamp,
                Id = Guid.NewGuid(),
                Level = dtoVisa.Level,
                Name = dtoVisa.Name
            };

            await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisa, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltPassport = await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisaToCreate, prvTime.GetUtcNow(), CancellationToken.None);

            // Assert
            rsltPassport.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not create visa {dtoVisaToCreate.Id}.");

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisa, CancellationToken.None);
        }
    }
}