using Passport.Application.Result;

namespace Passport.Application.Default
{
    public static class DefaultRepositoryError
    {
        public static RepositoryError TaskAborted => new RepositoryError() { Code = "TASK_ABORTED", Description = "Task has been cancelled." };
        public static RepositoryError ConcurrencyViolation => new RepositoryError() { Code = Code.Method, Description = "Data has been modified. Refresh and try again." };

        public static class Code
        {
            public static string Method = "METHOD_REPOSITORY";
            public static string Exception = "EXCEPTION_REPOSITORY";
        }
    }
}
