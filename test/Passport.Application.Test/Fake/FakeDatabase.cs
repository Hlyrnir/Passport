using Passport.Abstraction.Authentication;
using Passport.Application.Transfer;

namespace Passport.Application.Test.Fake
{
    internal sealed class FakeDatabase
    {
        public IDictionary<Guid, PassportTransferObject> Passport { get; } = new Dictionary<Guid, PassportTransferObject>();

        public IDictionary<Guid, PassportHolderTransferObject> Holder { get; } = new Dictionary<Guid, PassportHolderTransferObject>();

        public IDictionary<Guid, PassportTokenTransferObject> Token { get; } = new Dictionary<Guid, PassportTokenTransferObject>();
        public IDictionary<Guid, IPassportCredential> Credential { get; } = new Dictionary<Guid, IPassportCredential>();
        public IDictionary<Guid, int> FailedAttemptCounter { get; } = new Dictionary<Guid, int>();

        public IDictionary<Guid, PassportVisaTransferObject> Visa { get; } = new Dictionary<Guid, PassportVisaTransferObject>();
        public IDictionary<Guid, IList<Guid>> VisaRegister { get; } = new Dictionary<Guid, IList<Guid>>();
    }
}
