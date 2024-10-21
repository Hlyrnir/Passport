using FluentAssertions;
using Passport.Application.Extension;
using Passport.Application.Query.Passport.ById;
using Passport.Application.Result;

namespace Passport.Application.Test.Query.PassportById
{
    public class PassportByIdQueryHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportByIdQueryHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnPassport_WhenPassportExists()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportByIdQuery qryById = new PassportByIdQuery()
            {
                PassportId = ppPassport.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportByIdQueryHandler hdlQuery = new PassportByIdQueryHandler(
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<PassportByIdResult> rsltQuery = await hdlQuery.Handle(qryById, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                qryResult =>
                {
                    qryResult.Passport.Should().NotBeNull();
                    qryResult.Passport.Should().BeEquivalentTo(ppPassport);

                    return true;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenPassportDoesNotExist()
        {
            // Arrange
            PassportByIdQuery qryById = new PassportByIdQuery()
            {
                PassportId = Guid.NewGuid(),
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportByIdQueryHandler hdlQuery = new PassportByIdQueryHandler(
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<PassportByIdResult> rsltQuery = await hdlQuery.Handle(qryById, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.Passport.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.Passport.NotFound.Description);

                    return false;
                },
                ppPassportInRepository =>
                {
                    ppPassportInRepository.Should().BeNull();

                    return true;
                });
        }
    }
}