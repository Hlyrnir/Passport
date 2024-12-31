using Passport.Application.Result;

namespace Passport.Infrastructure.Test
{
    internal static class TestError
    {
        internal static class Repository
        {
            internal static class Domain
            {
                internal static RepositoryError NotInitialized = new RepositoryError() { Code = "TEST_METHOD_DOMAIN", Description = "Transfer object could not be initialized." };
            }

            internal static class Passport
            {
                internal static class Code
                {
                    public const string Method = "TEST_METHOD_PASSPORT";
                }

                public static RepositoryError Exists = new RepositoryError() { Code = Code.Method, Description = "Passport does already exist in repository." };
                public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Passport does not exist in repository." };
                public static RepositoryError VisaRegister = new RepositoryError() { Code = Code.Method, Description = "Could not register visa to passport." };
            }

            internal static class PassportHolder
            {
                internal static class Code
                {
                    public const string Method = "TEST_METHOD_PASSPORT_HOLDER";
                }

                public static RepositoryError Exists = new RepositoryError() { Code = Code.Method, Description = "Holder does already exist in repository." };
                public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Holder does not exist in repository." };
            }

            internal static class PassportToken
            {
                internal static class Code
                {
                    public const string Method = "TEST_METHOD_PASSPORT_TOKEN";
                }
                public static RepositoryError Exists = new RepositoryError() { Code = Code.Method, Description = "Token does already exist in repository." };
                public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Token does not exist in repository." };

                internal static class Credential
                {
                    public static RepositoryError Exists = new RepositoryError() { Code = Code.Method, Description = "Credential does already exist in repository." };
                    public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Credential does not exist in repository." };
                    public static RepositoryError Invalid = new RepositoryError() { Code = Code.Method, Description = "Credential and signature does not match." };
                }

                internal static class RefreshToken
                {
                    public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Refresh token does not exist in repository." };
                    public static RepositoryError Invalid = new RepositoryError() { Code = Code.Method, Description = "Refresh token does not match." };
                }

                internal static class FailedAttemptCounter
                {
                    public static RepositoryError NotAdded = new RepositoryError() { Code = Code.Method, Description = "Could not add failed attempt counter to passport." };
                    public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Could not found failed attempt counter of passport." };
                }
            }

            internal static class PassportVisa
            {
                internal static class Code
                {
                    public const string Method = "TEST_METHOD_PASSPORT_VISA";
                }
                public static RepositoryError Exists = new RepositoryError() { Code = Code.Method, Description = "Visa does already exist in repository." };
                public static RepositoryError NotFound = new RepositoryError() { Code = Code.Method, Description = "Visa does not exist in repository." };
                public static RepositoryError VisaRegister = new RepositoryError() { Code = Code.Method, Description = "No visa is registered to this passport." };
            }
        }
    }
}
