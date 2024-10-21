namespace Passport.Application.Credential
{
    public sealed class PassportHashSetting
    {
        public static string SectionName = "SignatureHash";

        private readonly string sPublicKey;

        public PassportHashSetting()
        {
            sPublicKey = string.Empty;
        }

        public string PublicKey { get => sPublicKey; init => sPublicKey = value; }
    }
}
