using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;

namespace Passport.Application.Validation
{
    internal sealed class PassportValidation : MessageValidation, IPassportValidation
    {
        private readonly IPassportSetting ppSetting;

        public PassportValidation(IPassportSetting ppSetting)
        {
            this.ppSetting = ppSetting;
        }

        public bool ValidateGuid(Guid guGuid, string sPropertyName)
        {
            if (Equals(guGuid, Guid.Empty) == true)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} is invalid (empty)." });
                return false;
            }

            return true;
        }

        public int ValidateEmailAddress(string sEmailAddress, string sPropertyName)
        {
            int iValidationErrorCounter = 0;

            if (string.IsNullOrWhiteSpace(sEmailAddress) == true)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Email address is not valid (empty)." });
                iValidationErrorCounter++;
                return iValidationErrorCounter;
            }

            ReadOnlySpan<char> cEmailAddress = sEmailAddress;
            int iIndexOfSpecial = cEmailAddress.IndexOf('@');

            if (iIndexOfSpecial != -1)
            {
                if (cEmailAddress.Slice(iIndexOfSpecial).IndexOf('.') == -1)
                {
                    Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Email address does not contain the '.' character." });
                    iValidationErrorCounter++;
                }
            }
            else
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Email address does not contain the '@' character." });
                iValidationErrorCounter++;
            }

            return iValidationErrorCounter;
        }

        public int ValidateCredential(string sCredential, string sPropertyName)
        {
            int iValidationErrorCounter = 0;

            if (string.IsNullOrWhiteSpace(sCredential) == true)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} is not valid (empty)." });
                iValidationErrorCounter++;
                return iValidationErrorCounter;
            }

            ReadOnlySpan<char> cCredential = sCredential;

            if (cCredential.Length < ppSetting.RequiredMinimalCredentialLength)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} has less than {ppSetting.RequiredMinimalCredentialLength} characters." });
                iValidationErrorCounter++;
            }

            if (cCredential.Length > ppSetting.MaximalCredentialLength)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} has more than {ppSetting.MaximalCredentialLength} characters." });
                iValidationErrorCounter++;
            }

            iValidationErrorCounter = iValidationErrorCounter + ValidateEmailAddress(sCredential, sPropertyName);

            return iValidationErrorCounter;
        }

        public int ValidatePhoneNumber(string sPhoneNumber, string sPropertyName)
        {
            int iValidationErrorCounter = 0;

            if (string.IsNullOrWhiteSpace(sPhoneNumber) == true)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} is not valid (empty)." });
                iValidationErrorCounter++;
                return iValidationErrorCounter;
            }

            ReadOnlySpan<char> cPhoneNumber = sPhoneNumber;

            if (cPhoneNumber.Length < ppSetting.MinimalPhoneNumberLength)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} has less than {ppSetting.MinimalPhoneNumberLength} characters." });
                iValidationErrorCounter++;
            }

            if (cPhoneNumber.IndexOfAny(ppSetting.RequiredDigit.AsSpan()) == -1)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} contains no digits." });
                iValidationErrorCounter++;
            }

            return iValidationErrorCounter;
        }

        public bool ValidateProvider(string sProvider, string sPropertyName)
        {
            if (string.IsNullOrWhiteSpace(sProvider) == true)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} is not valid (empty)." });
                return false;
            }

            bool bIsValid = false;

            foreach (string sActualProvider in ppSetting.ValidProviderName)
            {
                if (sActualProvider == sProvider)
                    bIsValid = true;
            }

            if (bIsValid == false)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} is unknown." });
                return false;
            }

            return true;
        }

        public int ValidateSignature(string sSignature, string sPropertyName)
        {
            int iValidationErrorCounter = 0;

            if (string.IsNullOrWhiteSpace(sSignature) == true)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} is not valid (empty)." });
                iValidationErrorCounter++;
                return iValidationErrorCounter;
            }

            ReadOnlySpan<char> cSignature = sSignature;

            if (cSignature.Length < ppSetting.RequiredMinimalSignatureLength)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} has less than {ppSetting.RequiredMinimalSignatureLength} characters." });
                iValidationErrorCounter++;
            }

            if (cSignature.Length > ppSetting.MaximalSignatureLength)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} has more than {ppSetting.MaximalSignatureLength} characters." });
                iValidationErrorCounter++;
            }

            if (cSignature.IndexOfAny(ppSetting.RequiredDigit.AsSpan()) == -1)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} contains no digits." });
                iValidationErrorCounter++;
            }

            if (cSignature.IndexOfAny(ppSetting.RequiredLowerCase.AsSpan()) == -1)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} contains no lower case." });
                iValidationErrorCounter++;
            }

            if (cSignature.IndexOfAny(ppSetting.RequiredUpperCase.AsSpan()) == -1)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} contains no upper case." });
                iValidationErrorCounter++;
            }

            if (cSignature.IndexOfAny(ppSetting.RequiredSpecial.AsSpan()) == -1)
            {
                Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"{sPropertyName} contains no specials ({ppSetting.RequiredSpecial})." });
                iValidationErrorCounter++;
            }

            return iValidationErrorCounter;
        }
    }
}
