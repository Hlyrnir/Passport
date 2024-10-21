namespace Passport.Contract.v01
{
    public abstract class PagedResponse
    {
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int ResultCount { get; init; }
    }
}
