using FluentAssertions;
using Passport.Application.Extension;
using Passport.Application.Query.PassportVisa.ById;
using Passport.Application.Result;

namespace Passport.Application.Test.Query.PassportVisaById
{
    public class PassportVisaByIdQueryHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportVisaByIdQueryHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Find_ShouldReturnPassport_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault();

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportVisaByIdQuery qryById = new PassportVisaByIdQuery()
            {
                PassportVisaId = ppVisa.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportVisaByIdQueryHandler hdlQuery = new PassportVisaByIdQueryHandler(fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<PassportVisaByIdResult> rsltQuery = await hdlQuery.Handle(qryById, CancellationToken.None);

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
                    qryResult.PassportVisa.Should().BeEquivalentTo(ppVisa);

                    return true;
                });

            //Clean up
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Find_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            PassportVisaByIdQuery qryById = new PassportVisaByIdQuery()
            {
                PassportVisaId = Guid.NewGuid(),
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportVisaByIdQueryHandler hdlQuery = new PassportVisaByIdQueryHandler(fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<PassportVisaByIdResult> rsltQuery = await hdlQuery.Handle(qryById, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportVisa.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportVisa.NotFound.Description);

                    return false;
                },
                ppPassportVisaInRepository =>
                {
                    ppPassportVisaInRepository.Should().BeNull();

                    return true;
                });
        }
    }
}