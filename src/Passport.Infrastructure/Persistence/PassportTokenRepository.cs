using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Passport.Abstraction.Authentication;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using Passport.Infrastructure.Name;
using Passport.Infrastructure.TypeHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Infrastructure.Persistence
{
    internal sealed class PassportTokenRepository : IPassportTokenRepository
    {
        private readonly IDataAccess sqlDataAccess;

        private readonly IPassportSetting ppSetting;
        private readonly IPassportHasher ppHasher;

        public PassportTokenRepository([FromKeyedServices(DefaultKeyedServiceName.DataAccess)] IDataAccess sqlDataAccess, IPassportSetting ppSetting, IPassportHasher ppHasher)
        {
            this.sqlDataAccess = sqlDataAccess;

            this.ppSetting = ppSetting;
            this.ppHasher = ppHasher;

            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> CredentialAtProviderExistsAsync(
            string sCredential,
            string sProvider,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT CASE WHEN EXISTS(
									SELECT 1 FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.Credential}] = @Credential 
									AND [{PassportTokenColumn.Provider}] = @Provider) 
									THEN 1 ELSE 0 END;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Credential", sCredential);
                dpParameter.Add("Provider", sProvider);

                bool bResult = await sqlDataAccess.Connection.ExecuteScalarAsync<bool>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                return new RepositoryResult<bool>(bResult);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<bool>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> DeleteAsync(
            PassportTokenTransferObject dtoPassportToken,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"DELETE FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", dtoPassportToken.Id);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not delete token of passport {dtoPassportToken.PassportId} at {dtoPassportToken.Provider}."
                    });

                return new RepositoryResult<bool>(true);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<bool>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> EnableTwoFactorAuthenticationAsync(
            PassportTokenTransferObject dtoPassportToken,
            bool bIsEnabled,
            DateTimeOffset dtEditedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
									[{PassportTokenColumn.EditedAt}] = @EditedAt, 
									[{PassportTokenColumn.TwoFactorIsEnabled}] = @IsEnabled, 
									[{PassportTokenColumn.RefreshToken}] = @RefreshToken 
									WHERE [{PassportTokenColumn.Id}] = @Id; 
                                    SELECT 
                                    [{PassportTokenColumn.TwoFactorIsEnabled}] 
									FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("EditedAt", dtEditedAt);
                dpParameter.Add("IsEnabled", bIsEnabled);
                dpParameter.Add("Id", dtoPassportToken.Id);
                dpParameter.Add("RefreshToken", Guid.NewGuid());

                bool bResult = await sqlDataAccess.Connection.ExecuteScalarAsync<bool>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (bResult != bIsEnabled)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Two factor authentication is enabled: {bResult}."
                    });

                return new RepositoryResult<bool>(bResult);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<bool>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<PassportTokenTransferObject>> FindTokenByCredentialAsync(
            IPassportCredential ppCredential,
            DateTimeOffset dtAttemptedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<PassportTokenTransferObject>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
                                    [{PassportTokenColumn.ExpiredAt}] = @ExpiredAt, 
									[{PassportTokenColumn.FailedAttemptCounter}] = @FailedAttemptCounter, 
									[{PassportTokenColumn.RefreshToken}] = @RefreshToken 
									WHERE [{PassportTokenColumn.FailedAttemptCounter}] <= @MaximalAllowedAccessAttempt 
									AND [{PassportTokenColumn.Credential}] = @Credential 
									AND [{PassportTokenColumn.Provider}] = @Provider 
									AND [{PassportTokenColumn.Signature}] = @Signature; 
									SELECT 
                                    [{PassportTokenColumn.ExpiredAt}], 
									[{PassportTokenColumn.Id}], 
									[{PassportTokenColumn.PassportId}], 
									[{PassportTokenColumn.Provider}], 
									[{PassportTokenColumn.RefreshToken}], 
									[{PassportTokenColumn.TwoFactorIsEnabled}] 
									FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.FailedAttemptCounter}] <= @MaximalAllowedAccessAttempt 
									AND [{PassportTokenColumn.Credential}] = @Credential 
									AND [{PassportTokenColumn.Provider}] = @Provider 
									AND [{PassportTokenColumn.Signature}] = @Signature;";

                DateTimeOffset dtExpiredAt = dtAttemptedAt.Add(ppSetting.RefreshTokenExpiresAfterDuration);

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Credential", ppCredential.Credential);
                dpParameter.Add("ExpiredAt", dtExpiredAt);
                dpParameter.Add("FailedAttemptCounter", 0); // Reset the failed attempt counter.
                dpParameter.Add("MaximalAllowedAccessAttempt", ppSetting.MaximalAllowedAccessAttempt);
                dpParameter.Add("Provider", ppCredential.Provider);
                dpParameter.Add("RefreshToken", Guid.NewGuid());
                dpParameter.Add("Signature", ppCredential.HashSignature(ppHasher));

                IEnumerable<PassportTokenTransferObject> enumTokenTransferObject = await sqlDataAccess.Connection.QueryAsync<PassportTokenTransferObject>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (enumTokenTransferObject.Count() < 1)
                    return new RepositoryResult<PassportTokenTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Credential and signature does not match at provider {ppCredential.Provider}."
                    });

                if (enumTokenTransferObject.Count() > 1)
                    return new RepositoryResult<PassportTokenTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could only find ambiguous results for credential at provider {ppCredential.Provider}."
                    });

                return new RepositoryResult<PassportTokenTransferObject>(enumTokenTransferObject.First());
            }
            catch (Exception exException)
            {
                return new RepositoryResult<PassportTokenTransferObject>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<PassportTokenTransferObject>> FindTokenByRefreshTokenAsync(
            Guid guPassportId,
            string sProvider,
            string sRefreshToken,
            DateTimeOffset dtAttemptedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<PassportTokenTransferObject>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
                                    [{PassportTokenColumn.ExpiredAt}] = @ExpiredAt, 
									[{PassportTokenColumn.FailedAttemptCounter}] = @FailedAttemptCounter, 
									[{PassportTokenColumn.RefreshToken}] = @RefreshToken 
									WHERE [{PassportTokenColumn.ExpiredAt}] > @AttemptedAt 
                                    AND [{PassportTokenColumn.FailedAttemptCounter}] <= @MaximalAllowedAccessAttempt 
									AND [{PassportTokenColumn.PassportId}] = @PassportId 
									AND [{PassportTokenColumn.Provider}] = @Provider 
									AND [{PassportTokenColumn.RefreshToken}] = @ActualToken; 
									SELECT 
                                    [{PassportTokenColumn.ExpiredAt}], 
									[{PassportTokenColumn.Id}], 
									[{PassportTokenColumn.PassportId}], 
									[{PassportTokenColumn.Provider}], 
									[{PassportTokenColumn.RefreshToken}], 
									[{PassportTokenColumn.TwoFactorIsEnabled}] 
									FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.ExpiredAt}] > @AttemptedAt 
                                    AND [{PassportTokenColumn.FailedAttemptCounter}] <= @MaximalAllowedAccessAttempt 
									AND [{PassportTokenColumn.PassportId}] = @PassportId 
									AND [{PassportTokenColumn.Provider}] = @Provider 
									AND [{PassportTokenColumn.RefreshToken}] = @RefreshToken;";

                DateTimeOffset dtExpiredAt = dtAttemptedAt.Add(ppSetting.RefreshTokenExpiresAfterDuration);

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ActualToken", sRefreshToken);
                dpParameter.Add("AttemptedAt", dtAttemptedAt);
                dpParameter.Add("ExpiredAt", dtExpiredAt);
                dpParameter.Add("FailedAttemptCounter", 0); // Reset the failed attempt counter.
                dpParameter.Add("MaximalAllowedAccessAttempt", ppSetting.MaximalAllowedAccessAttempt);
                dpParameter.Add("PassportId", guPassportId);
                dpParameter.Add("Provider", sProvider);
                dpParameter.Add("RefreshToken", Guid.NewGuid());

                IEnumerable<PassportTokenTransferObject> enumTokenTranferObject = await sqlDataAccess.Connection.QueryAsync<PassportTokenTransferObject>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (enumTokenTranferObject.Count() < 1)
                    return new RepositoryResult<PassportTokenTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Refresh token does not match at provider {sProvider}."
                    });

                if (enumTokenTranferObject.Count() > 1)
                    return new RepositoryResult<PassportTokenTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could only find ambiguous results for refresh token at provider {sProvider}."
                    });

                return new RepositoryResult<PassportTokenTransferObject>(enumTokenTranferObject.First());
            }
            catch (Exception exException)
            {
                return new RepositoryResult<PassportTokenTransferObject>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> InsertAsync(
            PassportTokenTransferObject dtoPassportToken,
            IPassportCredential ppCredential,
            DateTimeOffset dtCreatedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"INSERT INTO [{PassportTokenTable.PassportToken}](
										[{PassportTokenColumn.CreatedAt}], 
										[{PassportTokenColumn.Credential}], 
										[{PassportTokenColumn.EditedAt}], 
                                        [{PassportTokenColumn.ExpiredAt}], 
										[{PassportTokenColumn.FailedAttemptCounter}], 
										[{PassportTokenColumn.Id}], 
										[{PassportTokenColumn.LastFailedAt}], 
										[{PassportTokenColumn.PassportId}], 
										[{PassportTokenColumn.Provider}], 
										[{PassportTokenColumn.RefreshToken}], 
										[{PassportTokenColumn.Signature}], 
										[{PassportTokenColumn.TwoFactorIsEnabled}]) 
										SELECT
										@CreatedAt,
										@Credential,
										@EditedAt,
                                        @ExpiredAt,
										@FailedAttemptCounter,
										@Id,
										@LastFailedAt,
										@PassportId,
										@Provider,
										@RefreshToken,
										@Signature, 
										@TwoFactorIsEnabled 
										WHERE NOT EXISTS (
										SELECT 1
										FROM [{PassportTokenTable.PassportToken}] 
										WHERE [{PassportTokenColumn.PassportId}] = @PassportId 
										AND [{PassportTokenColumn.Provider}] != @Provider) 
										AND EXISTS (
										SELECT 1
										FROM [{PassportTable.Passport}] 
										WHERE [{PassportColumn.Id}] = @PassportId);";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("CreatedAt", dtCreatedAt);
                dpParameter.Add("Credential", ppCredential.Credential);
                dpParameter.Add("EditedAt", dtCreatedAt);
                dpParameter.Add("ExpiredAt", dtoPassportToken.ExpiredAt);
                dpParameter.Add("FailedAttemptCounter", 0);
                dpParameter.Add("Id", dtoPassportToken.Id);
                dpParameter.Add("LastFailedAt", dtCreatedAt);
                dpParameter.Add("PassportId", dtoPassportToken.PassportId);
                dpParameter.Add("Provider", dtoPassportToken.Provider);
                dpParameter.Add("RefreshToken", dtoPassportToken.RefreshToken);
                dpParameter.Add("Signature", ppCredential.HashSignature(ppHasher));
                dpParameter.Add("TwoFactorIsEnabled", dtoPassportToken.TwoFactorIsEnabled);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not create token for {dtoPassportToken.PassportId} at {dtoPassportToken.Provider}."
                    });

                return new RepositoryResult<bool>(true);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<bool>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> ResetCredentialAsync(
            PassportTokenTransferObject dtoPassportToken,
            IPassportCredential ppCredentialToApply,
            DateTimeOffset dtResetAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
									[{PassportTokenColumn.EditedAt}] = @EditedAt, 
                                    [{PassportTokenColumn.ExpiredAt}] = @ExpiredAt, 
									[{PassportTokenColumn.Signature}] = @Signature 
									WHERE [{PassportTokenColumn.Credential}] = @Credential 
									AND [{PassportTokenColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Credential", ppCredentialToApply.Credential);
                dpParameter.Add("EditedAt", dtResetAt);
                dpParameter.Add("ExpiredAt", dtoPassportToken.ExpiredAt);
                dpParameter.Add("Id", dtoPassportToken.Id);
                dpParameter.Add("Provider", dtoPassportToken.Provider);
                dpParameter.Add("Signature", ppCredentialToApply.HashSignature(ppHasher));

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Credential has not been reset at provider {ppCredentialToApply.Provider}."
                    });

                return new RepositoryResult<bool>(true);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<bool>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> ResetRefreshTokenAsync(
            Guid guPassportId,
            string sProvider,
            DateTimeOffset dtResetAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
									[{PassportTokenColumn.EditedAt}] = @EditedAt, 
                                    [{PassportTokenColumn.ExpiredAt}] = @ExpiredAt, 
									[{PassportTokenColumn.RefreshToken}] = @RefreshToken 
									WHERE [{PassportTokenColumn.PassportId}] = @PassportId 
									AND [{PassportTokenColumn.Provider}] = @Provider;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("EditedAt", dtResetAt);
                dpParameter.Add("ExpiredAt", dtResetAt);
                dpParameter.Add("RefreshToken", Guid.NewGuid());
                dpParameter.Add("PassportId", guPassportId);
                dpParameter.Add("Provider", sProvider);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Refresh token has not been reset at provider {sProvider}."
                    });

                return new RepositoryResult<bool>(true);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<bool>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<int>> VerifyCredentialAsync(
            IPassportCredential ppCredential,
            DateTimeOffset dtVerifiedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<int>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
									[{PassportTokenColumn.FailedAttemptCounter}] = [{PassportTokenColumn.FailedAttemptCounter}] + 1, 
									[{PassportTokenColumn.LastFailedAt}] = @LastFailedAt
									WHERE [{PassportTokenColumn.Credential}] = @Credential 
									AND [{PassportTokenColumn.Provider}] = @Provider 
									AND [{PassportTokenColumn.Signature}] != @Signature;
									SELECT 
									[{PassportTokenColumn.FailedAttemptCounter}] 
									FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.Credential}] = @Credential 
									AND [{PassportTokenColumn.Provider}] = @Provider;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Credential", ppCredential.Credential);
                dpParameter.Add("LastFailedAt", dtVerifiedAt);
                dpParameter.Add("Provider", ppCredential.Provider);
                dpParameter.Add("Signature", ppCredential.HashSignature(ppHasher));

                int iFailedAttemptCounter = await sqlDataAccess.Connection.ExecuteScalarAsync<int>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                return new RepositoryResult<int>(ppSetting.MaximalAllowedAccessAttempt + -1 * iFailedAttemptCounter);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<int>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<int>> VerifyRefreshTokenAsync(
            Guid guPassportId,
            string sProvider,
            string sRefreshToken,
            DateTimeOffset dtVerifiedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<int>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTokenTable.PassportToken}] SET 
									[{PassportTokenColumn.FailedAttemptCounter}] = [{PassportTokenColumn.FailedAttemptCounter}] + 1, 
									[{PassportTokenColumn.LastFailedAt}] = @LastFailedAt
									WHERE [{PassportTokenColumn.PassportId}] = @PassportId 
									AND [{PassportTokenColumn.Provider}] = @Provider 
									AND ([{PassportTokenColumn.ExpiredAt}] < @VerifiedAt 
                                    OR [{PassportTokenColumn.RefreshToken}] != @RefreshToken);
									SELECT 
									[{PassportTokenColumn.FailedAttemptCounter}] 
									FROM [{PassportTokenTable.PassportToken}] 
									WHERE [{PassportTokenColumn.PassportId}] = @PassportId 
									AND [{PassportTokenColumn.Provider}] = @Provider;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("LastFailedAt", dtVerifiedAt);
                dpParameter.Add("PassportId", guPassportId);
                dpParameter.Add("Provider", sProvider);
                dpParameter.Add("RefreshToken", sRefreshToken);
                dpParameter.Add("VerifiedAt", dtVerifiedAt);

                int iFailedAttemptCounter = await sqlDataAccess.Connection.ExecuteScalarAsync<int>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                return new RepositoryResult<int>(ppSetting.MaximalAllowedAccessAttempt + -1 * iFailedAttemptCounter);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<int>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }
    }
}