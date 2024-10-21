using FluentAssertions;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Register;
using Passport.Application.Default;

namespace Passport.Application.Test.Command.RegisterPassport
{
    public class RegisterPassportAuthorizationSpecification : IClassFixture<PassportFixture>
    {
        private readonly PassportFixture fxtPassport;
        private readonly TimeProvider prvTime;

        public RegisterPassportAuthorizationSpecification(PassportFixture fxtPassport)
        {
            this.fxtPassport = fxtPassport;
            prvTime = fxtPassport.TimeProvider;
        }

        [Fact]
        public async Task Register_ShouldReturnTrue_WhenPassportIdIsAuthorized()
        {
            // Arrange
            RegisterPassportCommand msgMessage = new RegisterPassportCommand()
            {
                CredentialToRegister = DataFaker.PassportCredential.CreateDefault(),
                CultureName = "en-GB",
                EmailAddress = "",
                FirstName = "",
                IssuedBy = Guid.NewGuid(),
                LastName = "",
                PhoneNumber = "",
                RestrictedPassportId = Guid.NewGuid()
            };

            IAuthorization<RegisterPassportCommand> msgAuthorization = new RegisterPassportAuthorization();

            // Act
            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(
                msgMessage: msgMessage,
                tknCancellation: CancellationToken.None);

            //Assert
            msgAuthorization.PassportVisaName.Should().Be(DefaultPassportVisa.Name.Passport);
            msgAuthorization.PassportVisaLevel.Should().Be(DefaultPassportVisa.Level.Create);

            rsltAuthorization.Match(
                msgError => false,
                bResult =>
                {
                    bResult.Should().BeTrue();

                    return true;
                });
        }
    }
}

//public class RegisterPassportAuthorizationSpecification : IClassFixture<PassportFixture>
//{
//    private readonly PassportFixture fxtPassport;
//    private readonly TimeProvider prvTime;

//    public RegisterPassportAuthorizationSpecification(PassportFixture fxtPassport)
//    {
//        this.fxtPassport = fxtPassport;
//        prvTime = fxtPassport.TimeProvider;
//    }

//    [Fact]
//    public async Task Register_ShouldReturnTrue_WhenPassportIdIsAuthorized()
//    {
//        // Arrange
//        Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault(DefaultPassportVisa.Name.Passport, DefaultPassportVisa.Level.Create);
//        Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
//        Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
//        IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

//        IEnumerable<Guid> enumPassportVisaId = new List<Guid>() { ppVisa.Id };
//        ppPassport.TryAddVisa(ppVisa);

//        await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
//        await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
//        await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

//        RegisterPassportCommand cmdRegister = new RegisterPassportCommand()
//        {
//            CredentialToRegister = ppCredential,
//            CultureName = "en-GB",
//            EmailAddress = "default@ema.il",
//            FirstName = "Jane",
//            IssuedBy = Guid.NewGuid(),
//            LastName = "Doe",
//            PhoneNumber = "111",
//            RestrictedPassportId = ppPassport.Id
//        };

//        IAuthorization<RegisterPassportCommand> hndlAuthorization = new RegisterPassportAuthorization();

//        // Act
//        IMessageResult<bool> rsltAuthorization = await hndlAuthorization.AuthorizeAsync(
//            msgMessage: cmdRegister,
//            tknCancellation: CancellationToken.None);

//        //Assert
//        rsltAuthorization.Match(
//            msgError => false,
//            bResult =>
//            {
//                bResult.Should().BeTrue();

//                return true;
//            });

//        //Clean up
//        await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
//        await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
//        await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
//    }

//    [Fact]
//    public async Task Register_ShouldReturnMessageError_WhenPassportVisaDoesNotExist()
//    {
//        // Arrange
//        Domain.Aggregate.PassportVisa ppVisa = DataFaker.PassportVisa.CreateDefault(DefaultPassportVisa.Name.Passport, DefaultPassportVisa.Level.Create);
//        Domain.Aggregate.Passport ppPassport = DataFaker.Passport.CreateDefault();
//        Domain.Aggregate.PassportToken ppToken = DataFaker.PassportToken.CreateDefault(ppPassport.Id);
//        IPassportCredential ppCredential = DataFaker.PassportCredential.CreateDefault();

//        IEnumerable<Guid> enumPassportVisaId = Enumerable.Empty<Guid>();
//        ppPassport.TryAddVisa(ppVisa);

//        await fxtPassport.PassportVisaRepository.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
//        await fxtPassport.PassportRepository.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), CancellationToken.None);
//        await fxtPassport.PassportTokenRepository.InsertAsync(ppToken.MapToTransferObject(), ppCredential, prvTime.GetUtcNow(), CancellationToken.None);

//        RegisterPassportCommand cmdRegister = new RegisterPassportCommand()
//        {
//            CredentialToRegister = ppCredential,
//            CultureName = "en-GB",
//            EmailAddress = "default@ema.il",
//            FirstName = "Jane",
//            IssuedBy = Guid.NewGuid(),
//            LastName = "Doe",
//            PhoneNumber = "111",
//            RestrictedPassportId = ppPassport.Id
//        };

//        IAuthorization<RegisterPassportCommand> hndlAuthorization = new RegisterPassportAuthorization();

//        // Act
//        IMessageResult<bool> rsltAuthorization = await hndlAuthorization.AuthorizeAsync(
//            msgMessage: cmdRegister,
//            tknCancellation: CancellationToken.None);

//        //Assert
//        rsltAuthorization.Match(
//            msgError => false,
//            bResult =>
//            {
//                bResult.Should().BeTrue();

//                return true;
//            });

//        //Clean up
//        await fxtPassport.PassportTokenRepository.DeleteAsync(ppToken.MapToTransferObject(), CancellationToken.None);
//        await fxtPassport.PassportRepository.DeleteAsync(ppPassport.MapToTransferObject(), CancellationToken.None);
//        await fxtPassport.PassportVisaRepository.DeleteAsync(ppVisa.MapToTransferObject(), CancellationToken.None);
//    }
//}