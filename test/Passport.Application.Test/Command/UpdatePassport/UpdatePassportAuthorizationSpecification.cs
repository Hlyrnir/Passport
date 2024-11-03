using FluentAssertions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Update;
using Passport.Application.Default;
using Passport.Application.Extension;

namespace Passport.Application.Test.Command.UpdatePassport
{
    public sealed class UpdatePassportAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public UpdatePassportAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Update_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateAuthority();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand msgMessage = new UpdatePassportCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                ExpiredAt = prvTime.GetUtcNow(),
                IsEnabled = false,
                IsAuthority = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = Guid.NewGuid(),
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            IAuthorization<UpdatePassportCommand> msgAuthorization = new UpdatePassportAuthorization(fxtPassport.PassportRepository);

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Update);

            rsltAuthorization.Match(
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
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenPassportIdIsNotEnabled()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand msgMessage = new UpdatePassportCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                ExpiredAt = prvTime.GetUtcNow(),
                IsEnabled = false,
                IsAuthority = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = Guid.NewGuid(),
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            IAuthorization<UpdatePassportCommand> msgAuthorization = new UpdatePassportAuthorization(fxtPassport.PassportRepository);

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Update);

            rsltAuthorization.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(AuthorizationError.Passport.IsDisabled);

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }

        [Fact]
        public async Task Update_ShouldReturnMessageError_WhenPassportIdIsNotAuthorized()
        {
            // Arrange
            Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
            Domain.Aggregate.Passport ppAuthority = DataFaker.Passport.CreateAuthority();
            IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

            ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow());
            await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);

            UpdatePassportCommand msgMessage = new UpdatePassportCommand()
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                ExpiredAt = prvTime.GetUtcNow(),
                IsEnabled = false,
                IsAuthority = false,
                LastCheckedAt = prvTime.GetUtcNow(),
                LastCheckedBy = Guid.NewGuid(),
                PassportIdToUpdate = Guid.NewGuid(),
                PassportVisaId = Enumerable.Empty<Guid>(),
                CredentialToVerify = ppCredential,
                RestrictedPassportId = ppPassport.Id
            };

            IAuthorization<UpdatePassportCommand> msgAuthorization = new UpdatePassportAuthorization(fxtPassport.PassportRepository);

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Update);

            rsltAuthorization.Match(
                msgError =>
                {
                    msgError.Should().NotBeNull();
                    msgError.Should().Be(AuthorizationError.Passport.NotAuthorized);

                    return false;
                },
                bResult =>
                {
                    bResult.Should().BeFalse();

                    return true;
                });

            // Clean up
            await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
        }
    }
}