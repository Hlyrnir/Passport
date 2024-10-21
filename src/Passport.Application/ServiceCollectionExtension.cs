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
using Passport.Domain;

namespace Passport.Application
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServiceCollection(this IServiceCollection cltService)
        {
            cltService.TryAddTransient<IPassportCredential, PassportCredential>();
            cltService.TryAddScoped<IPassportSetting, PassportSetting>();
            cltService.TryAddTransient<IPassportValidation, PassportValidation>();

            #region Authentication - Mediator

            //cltService.TryAddScoped<AuthenticationTokenByCredentialCommandHandler>();

            //cltService.Add(new ServiceDescriptor(
            //    typeof(ICommandHandler<AuthenticationTokenByCredentialCommand, MessageResult<AuthenticationTokenTransferObject>>),
            //    prvService => prvService.GetRequiredService<AuthenticationTokenByCredentialCommandHandler>(),
            //    ServiceLifetime.Scoped));
            #endregion

            #region Authentication - Validation
            cltService.AddScoped(typeof(IPipelineBehavior<AuthenticationTokenByCredentialCommand, IMessageResult<string>>), typeof(MessageValidationBehaviour<AuthenticationTokenByCredentialCommand, string>));
            cltService.TryAddTransient<IValidation<AuthenticationTokenByCredentialCommand>, AuthenticationTokenByCredentialValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<AuthenticationTokenByRefreshTokenCommand, IMessageResult<string>>), typeof(MessageValidationBehaviour<AuthenticationTokenByRefreshTokenCommand, string>));
            cltService.TryAddTransient<IValidation<AuthenticationTokenByRefreshTokenCommand>, AuthenticationTokenByRefreshTokenValidation>();
            #endregion

            #region Passport - Authorization
            cltService.AddScoped(typeof(IPipelineBehavior<RegisterPassportCommand, IMessageResult<Guid>>), typeof(MessageAuthorizationBehaviour<RegisterPassportCommand, Guid>));
            cltService.TryAddTransient<IAuthorization<RegisterPassportCommand>, RegisterPassportAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<SeizePassportCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<SeizePassportCommand, bool>));
            cltService.TryAddTransient<IAuthorization<SeizePassportCommand>, SeizePassportAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<UpdatePassportCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<UpdatePassportCommand, bool>));
            cltService.TryAddTransient<IAuthorization<UpdatePassportCommand>, UpdatePassportAuthorization>();
            #endregion

            #region Passport - Validation
            cltService.AddScoped(typeof(IPipelineBehavior<RegisterPassportCommand, IMessageResult<Guid>>), typeof(MessageValidationBehaviour<RegisterPassportCommand, Guid>));
            cltService.TryAddTransient<IValidation<RegisterPassportCommand>, RegisterPassportValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<SeizePassportCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<SeizePassportCommand, bool>));
            cltService.TryAddTransient<IValidation<SeizePassportCommand>, SeizePassportValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<UpdatePassportCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<UpdatePassportCommand, bool>));
            cltService.TryAddTransient<IValidation<UpdatePassportCommand>, UpdatePassportValidation>();
            #endregion

            #region PassportHolder - Authorization
            cltService.AddScoped(typeof(IPipelineBehavior<ConfirmEmailAddressCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<ConfirmEmailAddressCommand, bool>));
            cltService.TryAddTransient<IAuthorization<ConfirmEmailAddressCommand>, ConfirmEmailAddressAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<ConfirmPhoneNumberCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<ConfirmPhoneNumberCommand, bool>));
            cltService.TryAddTransient<IAuthorization<ConfirmPhoneNumberCommand>, ConfirmPhoneNumberAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<DeletePassportHolderCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<DeletePassportHolderCommand, bool>));
            cltService.TryAddTransient<IAuthorization<DeletePassportHolderCommand>, DeletePassportHolderAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<UpdatePassportHolderCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<UpdatePassportHolderCommand, bool>));
            cltService.TryAddTransient<IAuthorization<UpdatePassportHolderCommand>, UpdatePassportHolderAuthorization>();
            #endregion

            #region PassportHolder - Validation
            cltService.AddScoped(typeof(IPipelineBehavior<ConfirmEmailAddressCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<ConfirmEmailAddressCommand, bool>));
            cltService.TryAddTransient<IValidation<ConfirmEmailAddressCommand>, ConfirmEmailAddressValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<ConfirmPhoneNumberCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<ConfirmPhoneNumberCommand, bool>));
            cltService.TryAddTransient<IValidation<ConfirmPhoneNumberCommand>, ConfirmPhoneNumberValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<DeletePassportHolderCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<DeletePassportHolderCommand, bool>));
            cltService.TryAddTransient<IValidation<DeletePassportHolderCommand>, DeletePassportHolderValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<UpdatePassportHolderCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<UpdatePassportHolderCommand, bool>));
            cltService.TryAddTransient<IValidation<UpdatePassportHolderCommand>, UpdatePassportHolderValidation>();
            #endregion

            #region PassportToken - Authorization
            cltService.AddScoped(typeof(IPipelineBehavior<CreatePassportTokenCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<CreatePassportTokenCommand, bool>));
            cltService.TryAddTransient<IAuthorization<CreatePassportTokenCommand>, CreatePassportTokenAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<DeletePassportTokenCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<DeletePassportTokenCommand, bool>));
            cltService.TryAddTransient<IAuthorization<DeletePassportTokenCommand>, DeletePassportTokenAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<EnableTwoFactorAuthenticationCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<EnableTwoFactorAuthenticationCommand, bool>));
            cltService.TryAddTransient<IAuthorization<EnableTwoFactorAuthenticationCommand>, EnableTwoFactorAuthenticationAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<ResetCredentialCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<ResetCredentialCommand, bool>));
            cltService.TryAddTransient<IAuthorization<ResetCredentialCommand>, ResetCredentialAuthorization>();
            #endregion

            #region PassportToken - Validation
            cltService.AddScoped(typeof(IPipelineBehavior<CreatePassportTokenCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<CreatePassportTokenCommand, bool>));
            cltService.TryAddTransient<IValidation<CreatePassportTokenCommand>, CreatePassportTokenValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<DeletePassportTokenCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<DeletePassportTokenCommand, bool>));
            cltService.TryAddTransient<IValidation<DeletePassportTokenCommand>, DeletePassportTokenValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<EnableTwoFactorAuthenticationCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<EnableTwoFactorAuthenticationCommand, bool>));
            cltService.TryAddTransient<IValidation<EnableTwoFactorAuthenticationCommand>, EnableTwoFactorAuthenticationValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<ResetCredentialCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<ResetCredentialCommand, bool>));
            cltService.TryAddTransient<IValidation<ResetCredentialCommand>, ResetCredentialValidation>();
            #endregion

            #region PassportVisa - Authorization
            cltService.AddScoped(typeof(IPipelineBehavior<CreatePassportVisaCommand, IMessageResult<Guid>>), typeof(MessageAuthorizationBehaviour<CreatePassportVisaCommand, Guid>));
            cltService.TryAddTransient<IAuthorization<CreatePassportVisaCommand>, CreatePassportVisaAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<DeletePassportVisaCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<DeletePassportVisaCommand, bool>));
            cltService.TryAddTransient<IAuthorization<DeletePassportVisaCommand>, DeletePassportVisaAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<UpdatePassportVisaCommand, IMessageResult<bool>>), typeof(MessageAuthorizationBehaviour<UpdatePassportVisaCommand, bool>));
            cltService.TryAddTransient<IAuthorization<UpdatePassportVisaCommand>, UpdatePassportVisaAuthorization>();
            #endregion

            #region PassportVisa - Validation
            cltService.AddScoped(typeof(IPipelineBehavior<CreatePassportVisaCommand, IMessageResult<Guid>>), typeof(MessageValidationBehaviour<CreatePassportVisaCommand, Guid>));
            cltService.TryAddTransient<IValidation<CreatePassportVisaCommand>, CreatePassportVisaValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<DeletePassportVisaCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<DeletePassportVisaCommand, bool>));
            cltService.TryAddTransient<IValidation<DeletePassportVisaCommand>, DeletePassportVisaValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<UpdatePassportVisaCommand, IMessageResult<bool>>), typeof(MessageValidationBehaviour<UpdatePassportVisaCommand, bool>));
            cltService.TryAddTransient<IValidation<UpdatePassportVisaCommand>, UpdatePassportVisaValidation>();
            #endregion

            #region Passport - Authorization
            cltService.AddScoped(typeof(IPipelineBehavior<PassportByIdQuery, IMessageResult<PassportByIdResult>>), typeof(MessageAuthorizationBehaviour<PassportByIdQuery, PassportByIdResult>));
            cltService.TryAddTransient<IAuthorization<PassportByIdQuery>, PassportByIdAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<PassportHolderByIdQuery, IMessageResult<PassportHolderByIdResult>>), typeof(MessageAuthorizationBehaviour<PassportHolderByIdQuery, PassportHolderByIdResult>));
            cltService.TryAddTransient<IAuthorization<PassportHolderByIdQuery>, PassportHolderByIdAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<PassportVisaByIdQuery, IMessageResult<PassportVisaByIdResult>>), typeof(MessageAuthorizationBehaviour<PassportVisaByIdQuery, PassportVisaByIdResult>));
            cltService.TryAddTransient<IAuthorization<PassportVisaByIdQuery>, PassportVisaByIdAuthorization>();

            cltService.AddScoped(typeof(IPipelineBehavior<PassportVisaByPassportIdQuery, IMessageResult<PassportVisaByPassportIdResult>>), typeof(MessageAuthorizationBehaviour<PassportVisaByPassportIdQuery, PassportVisaByPassportIdResult>));
            cltService.TryAddTransient<IAuthorization<PassportVisaByPassportIdQuery>, PassportVisaByPassportIdAuthorization>();
            #endregion

            #region Passport - Validation
            cltService.AddScoped(typeof(IPipelineBehavior<PassportByIdQuery, IMessageResult<PassportByIdResult>>), typeof(MessageValidationBehaviour<PassportByIdQuery, PassportByIdResult>));
            cltService.TryAddTransient<IValidation<PassportByIdQuery>, PassportByIdValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<PassportHolderByIdQuery, IMessageResult<PassportHolderByIdResult>>), typeof(MessageValidationBehaviour<PassportHolderByIdQuery, PassportHolderByIdResult>));
            cltService.TryAddTransient<IValidation<PassportHolderByIdQuery>, PassportHolderByIdValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<PassportVisaByIdQuery, IMessageResult<PassportVisaByIdResult>>), typeof(MessageValidationBehaviour<PassportVisaByIdQuery, PassportVisaByIdResult>));
            cltService.TryAddTransient<IValidation<PassportVisaByIdQuery>, PassportVisaByIdValidation>();

            cltService.AddScoped(typeof(IPipelineBehavior<PassportVisaByPassportIdQuery, IMessageResult<PassportVisaByPassportIdResult>>), typeof(MessageValidationBehaviour<PassportVisaByPassportIdQuery, PassportVisaByPassportIdResult>));
            cltService.TryAddTransient<IValidation<PassportVisaByPassportIdQuery>, PassportVisaByPassportIdValidation>();
            #endregion

            return cltService;
        }
    }
}
