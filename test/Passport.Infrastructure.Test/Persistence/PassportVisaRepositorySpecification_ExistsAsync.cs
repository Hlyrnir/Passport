using FluentAssertions;
using Passport.Application.Extension;
using Passport.Application.Result;

namespace Passport.Infrastructure.Test.Repository
{
    public class PassportVisaRepositorySpecification_ExistsAsync : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;

        private readonly TimeProvider prvTime;

        public PassportVisaRepositorySpecification_ExistsAsync(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            this.prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Exists_ShouldReturnTrue_WhenAllVisaIdExist()
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
            RepositoryResult<bool> rsltVisa = await fxtPassport.PassportVisaRepository.ExistsAsync(ppPassport.VisaId, CancellationToken.None);

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
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_01.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_02.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_03.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_04.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Exists_ShouldReturnFalse_WhenNotAllVisaIdExist()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa_01 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_02 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_03 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_04 = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.PassportVisa ppVisa_DoesNotExists = DataFaker.PassportVisa.CreateDefault();

            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            ppPassport.TryAddVisa(ppVisa_01);
            ppPassport.TryAddVisa(ppVisa_02);
            ppPassport.TryAddVisa(ppVisa_03);
            ppPassport.TryAddVisa(ppVisa_04);
            ppPassport.TryAddVisa(ppVisa_DoesNotExists);

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_01.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_02.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_03.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa_04.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            // Act
            RepositoryResult<bool> rsltVisa = await fxtPassport.PassportVisaRepository.ExistsAsync(ppPassport.VisaId, CancellationToken.None);

            // Assert
            rsltVisa.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_01.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_02.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_03.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa_04.MapToTransferObject(), CancellationToken.None);
        }
    }
}
