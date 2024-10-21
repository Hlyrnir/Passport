using Passport.Application.Result;

namespace Passport.Application.Validation
{
    public interface IPassportValidation
    {
        bool IsValid { get; }

        R Match<R>(Func<MessageError, R> MethodIfIsFailed, Func<bool, R> MethodIfIsSuccess);

        bool Add(MessageError msgError);
        int ValidateCredential(string sCredential, string sPropertyName);
        int ValidateEmailAddress(string sEmailAddress, string sPropertyName);
        bool ValidateGuid(Guid guGuid, string sPropertyName);
        int ValidatePhoneNumber(string sPhoneNumber, string sPropertyName);
        bool ValidateProvider(string sProvider, string sPropertyName);
        int ValidateSignature(string sSignature, string sPropertyName);
    }
}
