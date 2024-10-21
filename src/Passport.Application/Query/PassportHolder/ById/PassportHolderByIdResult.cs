using Passport.Application.Transfer;

namespace Passport.Application.Query.PassportHolder.ById
{
    public sealed class PassportHolderByIdResult
    {
        public required PassportHolderTransferObject PassportHolder { get; init; }
    }
}
