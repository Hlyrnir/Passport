using FluentAssertions;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Authorization;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Test.Fake.Command;

namespace Passport.Application.Test.Behavior
{
    public class MessageAuthorizationBehaviorSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public MessageAuthorizationBehaviorSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Authorization_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            string sPassportVisaName = "TEST_AUTHORIZATION";
            int iPassportVisaLevel = 0;

            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault(sPassportVisaName, iPassportVisaLevel);
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            ppPassport.TryAddVisa(ppVisa);
            ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow());

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            FakeCommand cmdFake = new FakeCommand()
            {
                RestrictedPassportId = ppPassport.Id
            };

            FakeCommandHandler msgHandler = new FakeCommandHandler();

            IAuthorization<FakeCommand> msgAuthorization = new FakeAuthorization(
                sPassportVisaName: sPassportVisaName,
                iPassportVisaLevel: iPassportVisaLevel);

            MessageAuthorizationBehaviour<FakeCommand, FakeResult> msgBehavior = new MessageAuthorizationBehaviour<FakeCommand, FakeResult>(
                msgAuthorization: msgAuthorization,
                prvTime: fxtPassport.TimeProvider,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            IMessageResult<FakeResult> rsltAuthorization = await msgBehavior.Handle(cmdFake, CancellationToken.None, (FakeCommand msgMessage, CancellationToken tknCancellation) => { return msgHandler.Handle(msgMessage, tknCancellation); });

            //Assert
            rsltAuthorization.Match(
                msgError =>
                {
                    msgError.Should().BeNull();

                    return false;
                },
                msgResult =>
                {
                    msgResult.Should().NotBeNull();

                    return true;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Authorization_ShouldReturnMessageError_WhenPassportIdIsDisabled()
        {
            // Arrange
            string sPassportVisaName = "TEST_AUTHORIZATION";
            int iPassportVisaLevel = 0;

            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault(sPassportVisaName, iPassportVisaLevel);
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();

            ppPassport.TryAddVisa(ppVisa);

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            FakeCommand cmdFake = new FakeCommand()
            {
                RestrictedPassportId = ppPassport.Id
            };

            FakeCommandHandler msgHandler = new FakeCommandHandler();

            IAuthorization<FakeCommand> msgAuthorization = new FakeAuthorization(
                sPassportVisaName: sPassportVisaName,
                iPassportVisaLevel: iPassportVisaLevel);

            MessageAuthorizationBehaviour<FakeCommand, FakeResult> msgBehavior = new MessageAuthorizationBehaviour<FakeCommand, FakeResult>(
                msgAuthorization: msgAuthorization,
                prvTime: fxtPassport.TimeProvider,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            IMessageResult<FakeResult> rsltAuthorization = await msgBehavior.Handle(cmdFake, CancellationToken.None, (FakeCommand msgMessage, CancellationToken tknCancellation) => { return msgHandler.Handle(msgMessage, tknCancellation); });

            //Assert
            rsltAuthorization.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(AuthorizationError.Passport.IsDisabled);

                    return false;
                },
                msgResult =>
                {
                    msgResult.Should().BeNull();

                    return true;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Authorization_ShouldReturnMessageError_WhenPassportVisaNameDoesNotMatch()
        {
            // Arrange
            string sPassportVisaName = "TEST_AUTHORIZATION";
            string sInvalidPassportVisaName = "INVALID_AUTHORIZATION";
            int iPassportVisaLevel = 0;

            Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault(sPassportVisaName, iPassportVisaLevel);
            Domain.Aggregate.PassportVisa ppInvalidVisa = DataFaker.PassportVisa.CreateDefault(sInvalidPassportVisaName, iPassportVisaLevel);
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();

            ppPassport.TryAddVisa(ppInvalidVisa);
            ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow());

            await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.InsertAsync(ppInvalidVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            FakeCommand cmdFake = new FakeCommand()
            {
                RestrictedPassportId = ppPassport.Id
            };

            FakeCommandHandler msgHandler = new FakeCommandHandler();

            IAuthorization<FakeCommand> msgAuthorization = new FakeAuthorization(
                sPassportVisaName: sPassportVisaName,
                iPassportVisaLevel: iPassportVisaLevel);

            MessageAuthorizationBehaviour<FakeCommand, FakeResult> msgBehavior = new MessageAuthorizationBehaviour<FakeCommand, FakeResult>(
                msgAuthorization: msgAuthorization,
                prvTime: fxtPassport.TimeProvider,
                repoPassport: fxtPassport.PassportRepository,
                repoVisa: fxtPassport.PassportVisaRepository);

            // Act
            IMessageResult<FakeResult> rsltAuthorization = await msgBehavior.Handle(cmdFake, CancellationToken.None, (FakeCommand msgMessage, CancellationToken tknCancellation) => { return msgHandler.Handle(msgMessage, tknCancellation); });

            //Assert
            rsltAuthorization.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(AuthorizationError.PassportVisa.VisaDoesNotExist);

                    return false;
                },
                msgResult =>
                {
                    msgResult.Should().BeNull();

                    return true;
                });

            //Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
            await fxtPassport.PassportVisaRepository.DeleteAsync(ppInvalidVisa.MapToTransferObject(), CancellationToken.None);
        }
    }
}