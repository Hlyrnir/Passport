namespace Passport.Application.Result
{
    public readonly struct RepositoryError
    {
        public required string Code { get; init; }
        public required string Description { get; init; }
    }
}
