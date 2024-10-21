using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Result;

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

namespace Passport.Application.Test.Fake.Command
{
    internal class FakeValidation : IValidation<FakeCommand>
    {
        private string sTest;
        private IMessageError msgError;

        public FakeValidation(string sTest, IMessageError msgError)
        {
            this.sTest = sTest;
            this.msgError = msgError;
        }

        public async ValueTask<IMessageResult<bool>> ValidateAsync(FakeCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            if (msgMessage.Test != sTest)
                return new MessageResult<bool>(msgError);

            return new MessageResult<bool>(true);
        }
    }
}

#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

