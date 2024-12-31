using FluentAssertions;
using Passport.Application.Extension;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_FindByPassportAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_FindByPassportAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task FindByName_ShouldFindVisa_WhenVisaIsAddedToPassport()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            ppPassport.TryAddVisa(ppVisa);

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<IEnumerable<PassportVisaTransferObject>> rsltVisa = await fxtPassport.PassportVisaRepository.FindByPassportAsync(ppPassport.Id, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                enumPassportVisaInRepository =>
                {
                    enumPassportVisaInRepository.Should().ContainEquivalentOf(ppVisa.MapToTransferObject());

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task FindByName_ShouldFindAllVisa_WhenVisaAreAddedToPassport()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa_01 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_02 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_03 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_04 = DataFaker.PassportVisa.CreateDefault();

            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            ppPassport.TryAddVisa(ppVisa_01);
            ppPassport.TryAddVisa(ppVisa_02);
            ppPassport.TryAddVisa(ppVisa_03);
            ppPassport.TryAddVisa(ppVisa_04);

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_01.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_02.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_03.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_04.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<IEnumerable<PassportVisaTransferObject>> rsltVisa = await fxtPassport.PassportVisaRepository.FindByPassportAsync(ppPassport.Id, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                enumPassportVisaInRepository =>
                {
                    enumPassportVisaInRepository.Should().ContainEquivalentOf(ppVisa_01.MapToTransferObject());
                    enumPassportVisaInRepository.Should().ContainEquivalentOf(ppVisa_02.MapToTransferObject());
                    enumPassportVisaInRepository.Should().ContainEquivalentOf(ppVisa_03.MapToTransferObject());
                    enumPassportVisaInRepository.Should().ContainEquivalentOf(ppVisa_04.MapToTransferObject());

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_01.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_02.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_03.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_04.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task FindByName_ShouldFindNoVisa_WhenNoVisaIsAddedToPassport()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<IEnumerable<PassportVisaTransferObject>> rsltVisa = await fxtPassport.PassportVisaRepository.FindByPassportAsync(ppPassport.Id, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                enumPassportVisaInRepository =>
                {
                    enumPassportVisaInRepository.Should().BeEmpty();
                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}