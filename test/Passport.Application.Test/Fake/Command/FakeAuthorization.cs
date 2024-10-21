using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;


#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

namespace Passport.Application.Test.Fake.Command
{
    internal class FakeAuthorization : IAuthorization<FakeCommand>
    {
        private string sPassportVisaName;
        private int iPassportVisaLevel;

        public FakeAuthorization(string sPassportVisaName, int iPassportVisaLevel)
        {
            this.sPassportVisaName = sPassportVisaName;
            this.iPassportVisaLevel = iPassportVisaLevel;
        }

        public string PassportVisaName { get => sPassportVisaName; }
        public int PassportVisaLevel { get => iPassportVisaLevel; }

        public async ValueTask<IMessageResult<bool>> AuthorizeAsync(FakeCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}

#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
