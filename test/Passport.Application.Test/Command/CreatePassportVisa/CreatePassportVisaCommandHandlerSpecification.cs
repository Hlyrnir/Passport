using FluentAssertions;
using Passport.Application.Command.PassportVisa.Create;
using Passport.Application.Default;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Command.CreatePassportVisa
{
    public sealed class CreatePassportVisaCommandHandlerSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public CreatePassportVisaCommandHandlerSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Create_ShouldReturnTrue_WhenVisaIsCreated()
        {
            // Arrange
            CreatePassportVisaCommand cmdCreate = new CreatePassportVisaCommand()
            {
                Level = 0,
                Name = Guid.NewGuid().ToString(),
                RestrictedPassportId = Guid.NewGuid()
            };

            CreatePassportVisaCommandHandler cmdHandler = new CreatePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            MessageResult<Guid> rsltPassportVisaId = await cmdHandler.Handle(cmdCreate, CancellationToken.None);

            // Assert
            await rsltPassportVisaId.MatchAsync(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                async guPassportVisaId =>
                {
                    RepositoryResult<PassportVisaTransferObject> rsltPassportVisa = await fxtPassport.PassportVisaRepository.FindByIdAsync(guPassportVisaId, CancellationToken.None);

                    return rsltPassportVisa.Match(
                        msgError =>
                        {
                            msgError.Should().BeNull();

                            return false;
                        },
                        dtoPassportVisa =>
                        {
                            dtoPassportVisa.Level.Should().Be(cmdCreate.Level);
                            dtoPassportVisa.Name.Should().Be(cmdCreate.Name);

                            return true;
                        });
                });

            //Clean up
            RepositoryResult<PassportVisaTransferObject> rsltPassportVisa = await fxtPassport.PassportVisaRepository.FindByNameAsync(cmdCreate.Name, cmdCreate.Level, CancellationToken.None);

            await rsltPassportVisa.MatchAsync(
                msgError => false,
                async dtoVisa => await fxtPassport.PassportVisaRepository.DeleteAsync(dtoVisa, CancellationToken.None));
        }

        [Fact]
        public async Task Create_ShouldReturnRepositoryError_WhenVisaIsNotCreated()
        {
            // Arrange
            CreatePassportVisaCommand cmdCreate = new CreatePassportVisaCommand()
            {
                Level = -1,
                Name = Guid.NewGuid().ToString(),
                RestrictedPassportId = Guid.NewGuid()
            };

            // Act
            CreatePassportVisaCommandHandler cmdHandler = new CreatePassportVisaCommandHandler(
                prvTime: prvTime,
                repoVisa: fxtPassport.PassportVisaRepository);

            MessageResult<Guid> rsltPassportVisaId = await cmdHandler.Handle(cmdCreate, CancellationToken.None);

            // Assert
            rsltPassportVisaId.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Code.Should().Be(DomainError.Code.Method);
                    msgError.Description.Should().Be("Visa has not been created.");
                    return false;
                },
                guPassportVisaId =>
                {
                    guPassportVisaId.Should().BeEmpty();

                    return true;
                });
        }
    }
}