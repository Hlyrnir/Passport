using Dapper;
using Microsoft.Extensions.DependencyInjection;
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
    internal sealed class PassportRepository : IPassportRepository
    {
        private readonly IDataAccess sqlDataAccess;

        public PassportRepository([FromKeyedServices(DefaultKeyedServiceName.DataAccess)] IDataAccess sqlDataAccess)
        {
            this.sqlDataAccess = sqlDataAccess;

            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        public async Task<RepositoryResult<bool>> DeleteAsync(
            PassportTransferObject dtoPassport,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"DELETE FROM [{PassportTable.Passport}] 
									WHERE [{PassportColumn.ConcurrencyStamp}] = @ConcurrencyStamp 
									AND [{PassportColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ConcurrencyStamp", dtoPassport.ConcurrencyStamp);
                dpParameter.Add("Id", dtoPassport.Id);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not delete passport {dtoPassport.Id}."
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

        public async Task<RepositoryResult<bool>> ExistsAsync(
            Guid guPassportId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT CASE WHEN EXISTS(
									SELECT 1 FROM [{PassportTable.Passport}] 
									WHERE [{PassportColumn.Id}] = @Id) 
									THEN 1 ELSE 0 END;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", guPassportId);

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

        public async Task<RepositoryResult<PassportTransferObject>> FindByIdAsync(
            Guid guPassportId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<PassportTransferObject>(DefaultRepositoryError.TaskAborted);

            try
            {
                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", guPassportId);

                string sStatement = @$"SELECT 
					                [{PassportColumn.ConcurrencyStamp}], 
					                [{PassportColumn.ExpiredAt}], 
					                [{PassportColumn.HolderId}], 
					                [{PassportColumn.Id}], 
					                [{PassportColumn.IsAuthority}], 
					                [{PassportColumn.IsEnabled}], 
					                [{PassportColumn.IssuedBy}], 
					                [{PassportColumn.LastCheckedAt}], 
					                [{PassportColumn.LastCheckedBy}] 
					                FROM [{PassportTable.Passport}] 
					                WHERE [{PassportColumn.Id}] = @Id;
                                    SELECT 
					                [{PassportVisaRegisterColumn.PassportVisaId}] 
					                FROM 
					                [{PassportVisaRegisterTable.PassportVisaRegister}] 
					                WHERE [{PassportVisaRegisterColumn.PassportId}] = @Id;";

                SqlMapper.GridReader sqlGridReader = await sqlDataAccess.Connection.QueryMultipleAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                (string ConcurrencyStamp, DateTimeOffset ExpiredAt, Guid HolderId, Guid Id, bool IsAuthority, bool IsEnabled, Guid IssuedBy, DateTimeOffset LastCheckedAt, Guid LastCheckedBy) sqlData = await sqlGridReader.ReadSingleOrDefaultAsync<(string, DateTimeOffset, Guid, Guid, bool, bool, Guid, DateTimeOffset, Guid)>();
                IEnumerable<Guid> enumPassportVisaId = await sqlGridReader.ReadAsync<Guid>();

                if (sqlData.Equals(default))
                {
                    return new RepositoryResult<PassportTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"No data for {guPassportId} has been found."
                    });
                }

                PassportTransferObject dtoPassport = new PassportTransferObject()
                {
                    ConcurrencyStamp = sqlData.ConcurrencyStamp,
                    ExpiredAt = sqlData.ExpiredAt,
                    HolderId = sqlData.HolderId,
                    Id = sqlData.Id,
                    IsAuthority = sqlData.IsAuthority,
                    IsEnabled = sqlData.IsEnabled,
                    IssuedBy = sqlData.IssuedBy,
                    LastCheckedAt = sqlData.LastCheckedAt,
                    LastCheckedBy = sqlData.LastCheckedBy,
                    VisaId = enumPassportVisaId
                };

                return new RepositoryResult<PassportTransferObject>(dtoPassport);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<PassportTransferObject>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        public async Task<RepositoryResult<bool>> InsertAsync(
            PassportTransferObject dtoPassport,
            DateTimeOffset dtCreatedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"INSERT INTO [{PassportTable.Passport}](
									[{PassportColumn.ConcurrencyStamp}], 
									[{PassportColumn.CreatedAt}], 
									[{PassportColumn.EditedAt}], 
									[{PassportColumn.ExpiredAt}], 
									[{PassportColumn.HolderId}], 
									[{PassportColumn.Id}], 
									[{PassportColumn.IsAuthority}], 
									[{PassportColumn.IsEnabled}], 
									[{PassportColumn.IssuedBy}],
									[{PassportColumn.LastCheckedAt}], 
									[{PassportColumn.LastCheckedBy}]) 
									SELECT 
									@ConcurrencyStamp, 
									@CreatedAt, 
									@EditedAt, 
									@ExpiredAt, 
									@HolderId, 
									@Id, 
									@IsAuthority, 
									@IsEnabled, 
									@IssuedBy, 
									@LastCheckedAt, 
									@LastCheckedBy 
									WHERE NOT EXISTS (
									SELECT 1 
									FROM [{PassportTable.Passport}] 
									WHERE [{PassportColumn.Id}] = @Id);";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ConcurrencyStamp", dtoPassport.ConcurrencyStamp);
                dpParameter.Add("CreatedAt", dtCreatedAt);
                dpParameter.Add("EditedAt", dtCreatedAt);
                dpParameter.Add("ExpiredAt", dtoPassport.ExpiredAt);
                dpParameter.Add("HolderId", dtoPassport.HolderId);
                dpParameter.Add("Id", dtoPassport.Id);
                dpParameter.Add("IsAuthority", dtoPassport.IsAuthority);
                dpParameter.Add("IsEnabled", dtoPassport.IsEnabled);
                dpParameter.Add("IssuedBy", dtoPassport.IssuedBy);
                dpParameter.Add("LastCheckedAt", dtoPassport.LastCheckedAt);
                dpParameter.Add("LastCheckedBy", dtoPassport.LastCheckedBy);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not create passport {dtoPassport.Id}."
                    });

                if (dtoPassport.VisaId.Count() == 0)
                    return new RepositoryResult<bool>(true);

                bool bIsRegistered = await RegisterPassportVisaAsync(dtoPassport, dtCreatedAt, tknCancellation);

                if (bIsRegistered == false)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not register visa to passport {dtoPassport.Id}."
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

        public async Task<RepositoryResult<bool>> UpdateAsync(
            PassportTransferObject dtoPassport,
            DateTimeOffset dtEditedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportTable.Passport}] SET 
									[{PassportColumn.ConcurrencyStamp}] = @ConcurrencyStamp, 
									[{PassportColumn.EditedAt}] = @EditedAt, 
									[{PassportColumn.ExpiredAt}] = @ExpiredAt, 
									[{PassportColumn.HolderId}] = @HolderId, 
									[{PassportColumn.Id}] = @Id, 
									[{PassportColumn.IsAuthority}] = @IsAuthority, 
									[{PassportColumn.IsEnabled}] = @IsEnabled, 
									[{PassportColumn.IssuedBy}] = @IssuedBy, 
									[{PassportColumn.LastCheckedAt}] = @LastCheckedAt, 
									[{PassportColumn.LastCheckedBy}] = @LastCheckedBy 
									WHERE [{PassportColumn.ConcurrencyStamp}] = @ActualStamp 
									AND [{PassportColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ActualStamp", dtoPassport.ConcurrencyStamp);
                dpParameter.Add("ConcurrencyStamp", Guid.NewGuid());
                dpParameter.Add("EditedAt", dtEditedAt);
                dpParameter.Add("ExpiredAt", dtoPassport.ExpiredAt);
                dpParameter.Add("HolderId", dtoPassport.HolderId);
                dpParameter.Add("Id", dtoPassport.Id);
                dpParameter.Add("IsAuthority", dtoPassport.IsAuthority);
                dpParameter.Add("IsEnabled", dtoPassport.IsEnabled);
                dpParameter.Add("IssuedBy", dtoPassport.IssuedBy);
                dpParameter.Add("LastCheckedAt", dtoPassport.LastCheckedAt);
                dpParameter.Add("LastCheckedBy", dtoPassport.LastCheckedBy);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not update passport {dtoPassport.Id}."
                    });

                if (dtoPassport.VisaId.Count() == 0)
                    return new RepositoryResult<bool>(true);

                bool bIsRegistered = await RegisterPassportVisaAsync(dtoPassport, dtEditedAt, tknCancellation);

                if (bIsRegistered == false)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not register visa to passport {dtoPassport.Id}."
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

        private async Task<bool> RegisterPassportVisaAsync(
            PassportTransferObject dtoPassport,
            DateTimeOffset dtRegisteredAt,
            CancellationToken tknCancellation)
        {
            string sDeleteStatement = @$"DELETE FROM [{PassportVisaRegisterTable.PassportVisaRegister}] 
									WHERE [{PassportVisaRegisterColumn.PassportId}] = @Id 
									AND [{PassportVisaRegisterColumn.PassportVisaId}] NOT IN @VisaId;";

            DynamicParameters dpParameter = new DynamicParameters();
            dpParameter.Add("Id", dtoPassport.Id);
            dpParameter.Add("VisaId", dtoPassport.VisaId);

            await sqlDataAccess.Connection.ExecuteAsync(
                sql: sDeleteStatement,
                param: dpParameter,
                transaction: sqlDataAccess.Transaction);

            string sInsertStatement = @$"INSERT INTO [{PassportVisaRegisterTable.PassportVisaRegister}](
									[{PassportVisaRegisterColumn.Id}], 
									[{PassportVisaRegisterColumn.PassportId}], 
									[{PassportVisaRegisterColumn.PassportVisaId}], 
									[{PassportVisaRegisterColumn.RegisteredAt}]) 
									SELECT 
									@Id, 
									@PassportId, 
									@PassportVisaId, 
									@RegisteredAt 
									WHERE NOT EXISTS(
									SELECT 1 
									FROM [{PassportVisaRegisterTable.PassportVisaRegister}] 
									WHERE [{PassportVisaRegisterColumn.PassportId}] = @PassportId 
									AND [{PassportVisaRegisterColumn.PassportVisaId}] = @PassportVisaId)
									AND EXISTS( 
									SELECT 1 
									FROM [{PassportTable.Passport}] 
									WHERE [{PassportColumn.Id}] = @PassportId) 
									AND EXISTS( 
									SELECT 1 
									FROM[{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.Id}] = @PassportVisaId);";

            IList<DynamicParameters> lstParameter = new List<DynamicParameters>();

            foreach (Guid guVisaId in dtoPassport.VisaId)
            {
                DynamicParameters dpParameterToList = new DynamicParameters();
                dpParameterToList.Add("Id", Guid.NewGuid());
                dpParameterToList.Add("PassportId", dtoPassport.Id);
                dpParameterToList.Add("PassportVisaId", guVisaId);
                dpParameterToList.Add("RegisteredAt", dtRegisteredAt);

                lstParameter.Add(dpParameterToList);
            }

            await sqlDataAccess.Connection.ExecuteAsync(
                sql: sInsertStatement,
                param: lstParameter,
                transaction: sqlDataAccess.Transaction);

            return true;
        }
    }
}