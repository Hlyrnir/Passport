using Passport.Application.Default;
using Passport.Application.Result;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Passport.Application.Validation
{
    public class MessageValidation
    {
        private List<MessageError> lstValidationError;

        public MessageValidation()
        {
            lstValidationError = new List<MessageError>();
        }

        public bool IsValid
        {
            get
            {
                if (lstValidationError.Count == 0)
                    return true;

                return false;
            }
        }

        public R Match<R>(Func<MessageError, R> MethodIfIsFailed, Func<bool, R> MethodIfIsSuccess)
        {
            if (MethodIfIsSuccess is null || MethodIfIsFailed is null)
                throw new NotImplementedException("Match function is not defined.");

            if (IsValid)
                return MethodIfIsSuccess(true);

            return MethodIfIsFailed(Summary());
        }

        public bool Add(MessageError msgError)
        {
            lstValidationError.Add(msgError);

            return true;
        }

        private MessageError Summary()
        {
            JsonSerializerOptions jsonOption = new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            };

            return new MessageError()
            {
                Code = ValidationError.Code.Method,
                Description = JsonSerializer.Serialize(lstValidationError, jsonOption)
            };
        }
    }
}
