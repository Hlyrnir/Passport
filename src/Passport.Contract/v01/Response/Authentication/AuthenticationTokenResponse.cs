namespace Passport.Contract.v01.Response.Authentication
{
    public class AuthenticationTokenResponse
    {
        public required DateTimeOffset ExpiredAt { get; init; }
        public required string Provider { get; init; }
        public required string Token { get; init; }
        public required string RefreshToken { get; init; }
    }
}
