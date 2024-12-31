using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_FindByIdAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_FindByIdAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldFindVisa_WhenVisaIdExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByIdAsync(ppVisa.Id, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                dtoPassportVisaInRepository =>
                {
                    dtoPassportVisaInRepository.Should().NotBeNull();
                    dtoPassportVisaInRepository.Should().BeEquivalentTo(ppVisa);

                    return true;
                });

            // Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Find_ShouldReturnRepositoryError_WhenVisaIdDoesNotExist()
        {
            // Arrange
            Guid guVisaId = Guid.NewGuid();

            // Act
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByIdAsync(guVisaId, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"No data for {guVisaId} has been found.");

                    return false;
                },
                dtoPassportVisaInRepository =>
                {
                    dtoPassportVisaInRepository.Should().BeNull();

                    return true;
                });
        }
    }
}
