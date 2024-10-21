using Passport.Abstraction.Authentication;
using System.Text;

namespace Passport.DataFaker
{
    internal class FakePassportCredential : IPassportCredential
    {
        private bool bIsInitialized;

        public FakePassportCredential()
        {
            this.bIsInitialized = false;
        }

        public string Provider { get; private set; } = string.Empty;

        public string Credential { get; private set; } = string.Empty;

        public string Signature { get; private set; } = string.Empty;

        public byte[] HashSignature(IPassportHasher ppHasher)
        {
            return Encoding.UTF8.GetBytes(this.Signature);
        }

        public bool Initialize(string sProvider, string sCredential, string sSignature)
        {
            if (string.IsNullOrWhiteSpace(sProvider) == true)
                return false;

            if (string.IsNullOrWhiteSpace(sCredential) == true)
                return false;

            if (string.IsNullOrWhiteSpace(sSignature) == true)
                return false;

            if (bIsInitialized == true)
                return false;

            this.Provider = sProvider;
            this.Credential = sCredential;
            this.Signature = sSignature;

            this.bIsInitialized = true;

            return true;
        }
    }
}