using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_DeleteAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_DeleteAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenVisaIsDeleted()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltVisa = await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);

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
        }

        [Fact]
        public async Task Delete_ShouldReturnRepositoryError_WhenConcurrencyStampIsDifferent()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            PassportVisaTransferObject dtoVisa = ppVisa.MapToTransferObject();
            PassportVisaTransferObject dtoVisaToDelete = dtoVisa.Clone(bResetConcurrencyStamp: true);

            await fxtPassport.PassportVisaRepository.InsertAsync(dtoVisa, prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltVisa = await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisaToDelete, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"Could not delete visa {dtoVisaToDelete.Id}.");

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