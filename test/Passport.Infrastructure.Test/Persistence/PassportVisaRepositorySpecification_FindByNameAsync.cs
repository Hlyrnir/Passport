using FluentAssertions;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_FindByNameAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_FindByNameAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldFindVisa_WhenNameExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByNameAsync(ppVisa.Name, ppVisa.Level, CancellationToken.None);

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
        public async Task Find_ShouldReturnRepositoryError_WhenNameDoesNotExist()
        {
            // Arrange
            string sName = "DEFAULT_NAME";
            int iLevel = 0;

            // Act
            RepositoryResult<PassportVisaTransferObject> rsltVisa = await fxtPassport.PassportVisaRepository.FindByNameAsync(sName, iLevel, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DefaultRepositoryError.Code.Method);
                    msgError.Description.Should().Be($"No data for visa of name {sName} at level {iLevel} has been found.");

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