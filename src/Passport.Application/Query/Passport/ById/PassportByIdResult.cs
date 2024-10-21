using Passport.Application.Transfer;

namespace Passport.Application.Query.Passport.ById
{
    public sealed class PassportByIdResult
    {
        public required PassportTransferObject Passport { get; init; }
    }
}
