using FluentAssertions;
using Passport.Application.Extension;
using Passport.Application.Query.PassportHolder.ById;
using Passport.Application.Result;

namespace Passport.Application.Test.Query.PassportHolderById
{
    public class PassportHolderByIdQueryHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public PassportHolderByIdQueryHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnPassportHolder_WhenHolderExists()
        {
            // Arrange
            Domain.Aggregate.PassportHolder ppHolder = DataFaker.PassportHolder.CreateDefault();

            await fxtPassport.PassportHolderRepository.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            PassportHolderByIdQuery qryById = new PassportHolderByIdQuery()
            {
                PassportHolderId = ppHolder.Id,
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportHolderByIdQueryHandler hdlQuery = new PassportHolderByIdQueryHandler(
                ppSetting: fxtPassport.PassportSetting,
                repoHolder: fxtPassport.PassportHolderRepository);

            // Act
            MessageResult<PassportHolderByIdResult> rsltQuery = await hdlQuery.Handle(qryById, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                qryResult =>
                {
                    qryResult.PassportHolder.Should().NotBeNull();
                    qryResult.PassportHolder.Should().BeEquivalentTo(ppHolder);

                    return true;
                });

            //Clean up
            await fxtPassport.PassportHolderRepository.DeleteAsync(ppHolder.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenHolderDoesNotExist()
        {
            // Arrange
            PassportHolderByIdQuery qryById = new PassportHolderByIdQuery()
            {
                PassportHolderId = Guid.NewGuid(),
                RestrictedPassportId = Guid.NewGuid()
            };

            PassportHolderByIdQueryHandler hdlQuery = new PassportHolderByIdQueryHandler(
                ppSetting: fxtPassport.PassportSetting,
                repoHolder: fxtPassport.PassportHolderRepository);

            // Act
            MessageResult<PassportHolderByIdResult> rsltQuery = await hdlQuery.Handle(qryById, CancellationToken.None);

            //Assert
            rsltQuery.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(TestError.Repository.PassportHolder.NotFound.Code);
                    msgError.Description.Should().Be(TestError.Repository.PassportHolder.NotFound.Description);

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