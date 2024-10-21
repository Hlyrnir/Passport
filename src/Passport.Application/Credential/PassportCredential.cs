using Microsoft.AspNetCore.DataProtection;
using Passport.Abstraction.Authentication;

namespace Passport.Application.Credential
{
    public sealed class PassportCredential : IPassportCredential
    {
        private IDataProtector dpProtector;

        private string sProvider;
        private string sCredential;
        private string sSignature;

        public PassportCredential(IDataProtector dpProtector)
        {
            this.dpProtector = dpProtector;

            sProvider = string.Empty;
            sCredential = string.Empty;
            sSignature = string.Empty;
        }

        public string Provider { get => sProvider; }
        public string Credential { get => dpProtector.Unprotect(sCredential); }
        public string Signature { get => dpProtector.Unprotect(sSignature); }

        public bool Initialize(string sProvider, string sCredential, string sSignature)
        {
            if (string.IsNullOrWhiteSpace(sProvider) == true)
                return false;

            if (string.IsNullOrWhiteSpace(sCredential) == true)
                return false;

            if (string.IsNullOrWhiteSpace(sSignature) == true)
                return false;

            this.sProvider = sProvider;
            this.sCredential = dpProtector.Protect(sCredential);
            this.sSignature = dpProtector.Protect(sSignature);

            return true;
        }

        public byte[] HashSignature(IPassportHasher ppHasher)
        {
            return ppHasher.HashSignature(dpProtector.Unprotect(sSignature));
        }
    }
}