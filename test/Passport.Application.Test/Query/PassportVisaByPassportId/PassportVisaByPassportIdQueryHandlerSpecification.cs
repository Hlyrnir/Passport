using FluentAssertions;
using Passport.Application.Extension;
using Passport.Application.Query.PassportVisa.ByPassportId;
using Passport.Application.Result;

namespace Passport.Application.Test.Query.PassportVisaByPassportId
{
    public class PassportVisaByPassportIdQueryHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportVisaByPassportIdQueryHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldReturnPassportVisa_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            ppPassport.TryAddVisa(ppVisa);

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportVisaByPassportIdQuery qryByPassportId = new PassportVisaByPassportIdQuery()
            {
                PassportIdToFind = ppPassport.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportVisaByPassportIdQueryHandler hdlQuery = new PassportVisaByPassportIdQueryHandler(fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<PassportVisaByPassportIdResult> rsltQuery = await hdlQuery.Handle(qryByPassportId, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                qryResult =>
                {
                    qryResult.PassportVisa.Should().NotBeNull();
                    qryResult.PassportVisa.Should().ContainEquivalentOf(ppVisa);

                    return true;
                });

            //Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Find_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            PassportVisaByPassportIdQuery qryByPassportId = new PassportVisaByPassportIdQuery()
            {
                PassportIdToFind = Guid.NewGuid(),
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportVisaByPassportIdQueryHandler hdlQuery = new PassportVisaByPassportIdQueryHandler(fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<PassportVisaByPassportIdResult> rsltQuery = await hdlQuery.Handle(qryByPassportId, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportVisa.VisaRegister.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportVisa.VisaRegister.Description);

                    return false;
                },
                rsltPassportVisaInRepository =>
                {
                    rsltPassportVisaInRepository.Should().BeNull();

                    return true;
                });
        }
    }
}