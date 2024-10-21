namespace Passport.Contract.v01
{
    public abstract class PagedRequest
    {
        public required int Page { get; init; } = 1;
        public required int PageSize { get; init; } = 10;
    }
}
