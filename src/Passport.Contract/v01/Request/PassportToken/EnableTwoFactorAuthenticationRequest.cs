namespace Passport.Contract.v01.Request.PassportToken
{
    public sealed class EnableTwoFactorAuthenticationRequest : VerifiedRequest
    {
        public required bool TwoFactorIsEnabled { get; init; }
    }
}
