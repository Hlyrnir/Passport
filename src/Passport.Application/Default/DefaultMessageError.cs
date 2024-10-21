using Passport.Application.Result;

namespace Passport.Application.Default
{
    public static class DefaultMessageError
    {
        public static MessageError TaskAborted = new MessageError() { Code = "TASK_ABORTED", Description = "Task has been cancelled." };
        public static MessageError ConcurrencyViolation = new MessageError() { Code = "METHOD_APPLICATION", Description = "Data has been modified. Refresh and try again." };
    }

    public static class AuthenticationError
    {
        public static class Code
        {
            public static string Method = "METHOD_AUTHENTICATION";
        }

        public static MessageError CredentialNotFound = new MessageError() { Code = Code.Method, Description = "Credential could not be found." };
        public static MessageError TooManyAttempts = new MessageError() { Code = Code.Method, Description = "Credential has been blocked due too many failed attempts." };
        public static MessageError InvalidAuthenticationToken = new MessageError() { Code = Code.Method, Description = "Authentication token could not be generated." };
    }

    public static class AuthorizationError
    {
        public static class Code
        {
            public static string Method = "METHOD_AUTHORIZATION";
        }

        public static class VerifiedAuthorization
        {
            public static MessageError NotVerified = new MessageError() { Code = Code.Method, Description = "Passport is not authorized for this request." };
        }

        public static class Passport
        {
            public static MessageError IsDisabled = new MessageError() { Code = Code.Method, Description = "Passport is disabled." };
            public static MessageError IsExpired = new MessageError() { Code = Code.Method, Description = "Passport is expired." };
        }

        public static class PassportVisa
        {
            public static MessageError VisaDoesNotExist = new MessageError() { Code = Code.Method, Description = "Passport has no valid visa for this request." };
        }
    }

    internal static class ValidationError
    {
        internal static class Code
        {
            public static string Method = "METHOD_VALIDATION";
        }

        internal static class Authentication
        {
            public static MessageError InvalidRefreshToken = new MessageError() { Code = Code.Method, Description = "Refresh token is invalid." };

        }

        internal static class Passport
        {
            public static MessageError IsInvalid = new MessageError() { Code = Code.Method, Description = "Validation rules have been violated." };
            public static MessageError InvalidRefreshToken = new MessageError() { Code = Code.Method, Description = "Refresh token is invalid." };
        }
    }

    internal static class DomainError
    {
        internal static class Code
        {
            public static string Method = "METHOD_DOMAIN";
        }

        public static MessageError InitializationHasFailed = new MessageError() { Code = Code.Method, Description = "Transfer object could not be initialized." };
    }
}