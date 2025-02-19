using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Authorization;
using Passport.Application.Command.Authentication.ByCredential;
using Passport.Application.Command.Authentication.ByRefreshToken;
using Passport.Application.Command.Passport.Register;
using Passport.Application.Command.Passport.Seize;
using Passport.Application.Command.Passport.Update;
using Passport.Application.Command.PassportHolder.ConfirmEmailAddress;
using Passport.Application.Command.PassportHolder.ConfirmPhoneNumber;
using Passport.Application.Command.PassportHolder.Delete;
using Passport.Application.Command.PassportHolder.Update;
using Passport.Application.Command.PassportToken.Create;
using Passport.Application.Command.PassportToken.Delete;
using Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication;
using Passport.Application.Command.PassportToken.ResetCredential;
using Passport.Application.Command.PassportVisa.Create;
using Passport.Application.Command.PassportVisa.Delete;
using Passport.Application.Command.PassportVisa.Update;
using Passport.Application.Credential;
using Passport.Application.Query.Passport.ById;
using Passport.Application.Query.PassportHolder.ById;
using Passport.Application.Query.PassportVisa.ById;
using Passport.Application.Query.PassportVisa.ByPassportId;
using Passport.Application.Validation;
using System;

namespace Passport.Application
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServiceCollection(this IServiceCollection cltService)
        {
            cltService.TryAddTransient<IPassportCredential, PassportCredential>();
            cltService.TryAddTransient<IPassportValidation, PassportValidation>();

            #region Authentication - Command
            cltService.AddScoped(typeof(IPipelineBehavior<AuthenticationTokenByCredentialCommand, IMessageResult<string>>), typeof(MessageValidationBehaviour<AuthenticationTokenByCredentialCommand, string>));
            cltService.TryAddTransient<IValidation<AuthenticationTokenByCredentialCommand>, AuthenticationTokenByCredentialValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<AuthenticationTokenByRefreshTokenCommand, IMessageResult<string>>), typeof(MessageValidationBehaviour<AuthenticationTokenByRefreshTokenCommand, string>));
            cltService.TryAddTransient<IValidation<AuthenticationTokenByRefreshTokenCommand>, AuthenticationTokenByRefreshTokenValidation>();
            #endregion

            #region Passport - Command
            cltService.AddAuthorizationBehaviour<RegisterPassportCommand, Guid, RegisterPassportAuthorization>();
            cltService.AddValidationBehaviour<RegisterPassportCommand, Guid, RegisterPassportValidation>();

            cltService.AddAuthorizationBehaviour<SeizePassportCommand, bool, SeizePassportAuthorization>();
            cltService.AddValidationBehaviour<SeizePassportCommand, bool, SeizePassportValidation>();

            cltService.AddAuthorizationBehaviour<UpdatePassportCommand, bool, UpdatePassportAuthorization>();
            cltService.AddValidationBehaviour<UpdatePassportCommand, bool, UpdatePassportValidation>();
            #endregion

            #region PassportHolder - Command
            cltService.AddAuthorizationBehaviour<ConfirmEmailAddressCommand, bool, ConfirmEmailAddressAuthorization>();
            cltService.AddValidationBehaviour<ConfirmEmailAddressCommand, bool, ConfirmEmailAddressValidation>();

            cltService.AddAuthorizationBehaviour<ConfirmPhoneNumberCommand, bool, ConfirmPhoneNumberAuthorization>();
            cltService.AddValidationBehaviour<ConfirmPhoneNumberCommand, bool, ConfirmPhoneNumberValidation>();

            cltService.AddAuthorizationBehaviour<DeletePassportHolderCommand, bool, DeletePassportHolderAuthorization>();
            cltService.AddValidationBehaviour<DeletePassportHolderCommand, bool, DeletePassportHolderValidation>();

            cltService.AddAuthorizationBehaviour<UpdatePassportHolderCommand, bool, UpdatePassportHolderAuthorization>();
            cltService.AddValidationBehaviour<UpdatePassportHolderCommand, bool, UpdatePassportHolderValidation>();
            #endregion

            #region PassportToken - Command
            cltService.AddAuthorizationBehaviour<CreatePassportTokenCommand, bool, CreatePassportTokenAuthorization>();
            cltService.AddValidationBehaviour<CreatePassportTokenCommand, bool, CreatePassportTokenValidation>();

            cltService.AddAuthorizationBehaviour<DeletePassportTokenCommand, bool, DeletePassportTokenAuthorization>();
            cltService.AddValidationBehaviour<DeletePassportTokenCommand, bool, DeletePassportTokenValidation>();

            cltService.AddAuthorizationBehaviour<EnableTwoFactorAuthenticationCommand, bool, EnableTwoFactorAuthenticationAuthorization>();
            cltService.AddValidationBehaviour<EnableTwoFactorAuthenticationCommand, bool, EnableTwoFactorAuthenticationValidation>();

            cltService.AddAuthorizationBehaviour<ResetCredentialCommand, bool, ResetCredentialAuthorization>();
            cltService.AddValidationBehaviour<ResetCredentialCommand, bool, ResetCredentialValidation>();
            #endregion

            #region PassportVisa - Command
            cltService.AddAuthorizationBehaviour<CreatePassportVisaCommand, Guid, CreatePassportVisaAuthorization>();
            cltService.AddValidationBehaviour<CreatePassportVisaCommand, Guid, CreatePassportVisaValidation>();

            cltService.AddAuthorizationBehaviour<DeletePassportVisaCommand, bool, DeletePassportVisaAuthorization>();
            cltService.AddValidationBehaviour<DeletePassportVisaCommand, bool, DeletePassportVisaValidation>();

            cltService.AddAuthorizationBehaviour<UpdatePassportVisaCommand, bool, UpdatePassportVisaAuthorization>();
            cltService.AddValidationBehaviour<UpdatePassportVisaCommand, bool, UpdatePassportVisaValidation>();
            #endregion

            #region Passport - Query
            cltService.AddAuthorizationBehaviour<PassportByIdQuery, PassportByIdResult, PassportByIdAuthorization>();
            cltService.AddValidationBehaviour<PassportByIdQuery, PassportByIdResult, PassportByIdValidation>();
            #endregion

            #region PassportHolder - Query
            cltService.AddAuthorizationBehaviour<PassportHolderByIdQuery, PassportHolderByIdResult, PassportHolderByIdAuthorization>();
            cltService.AddValidationBehaviour<PassportHolderByIdQuery, PassportHolderByIdResult, PassportHolderByIdValidation>();
            #endregion

            #region PassportVisa - Query
            cltService.AddAuthorizationBehaviour<PassportVisaByIdQuery, PassportVisaByIdResult, PassportVisaByIdAuthorization>();
            cltService.AddValidationBehaviour<PassportVisaByIdQuery, PassportVisaByIdResult, PassportVisaByIdValidation>();

            cltService.AddAuthorizationBehaviour<PassportVisaByPassportIdQuery, PassportVisaByPassportIdResult, PassportVisaByPassportIdAuthorization>();
            cltService.AddValidationBehaviour<PassportVisaByPassportIdQuery, PassportVisaByPassportIdResult, PassportVisaByPassportIdValidation>();
            #endregion

            return cltService;
        }

        public static IServiceCollection AddAuthorizationBehaviour<TMessage, TResponse, TAuthorization>(this IServiceCollection cltService)
            where TMessage : notnull, IMessage, IRestrictedAuthorization
            where TAuthorization : IAuthorization<TMessage>
        {
            cltService.AddScoped(typeof(IPipelineBehavior<TMessage, IMessageResult<TResponse>>), typeof(MessageAuthorizationBehaviour<TMessage, TResponse>));
            cltService.TryAddTransient(typeof(IAuthorization<TMessage>), typeof(TAuthorization));

            return cltService;
        }

        public static IServiceCollection AddValidationBehaviour<TMessage, TResponse, TValidation>(this IServiceCollection cltService)
            where TMessage : notnull, IMessage
            where TValidation : IValidation<TMessage>
        {
            cltService.AddScoped(typeof(IPipelineBehavior<TMessage, IMessageResult<TResponse>>), typeof(MessageValidationBehaviour<TMessage, TResponse>));
            cltService.TryAddTransient(typeof(IValidation<TMessage>), typeof(TValidation));

            return cltService;
        }
    }
}