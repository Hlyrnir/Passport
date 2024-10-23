using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using Passport.Infrastructure.Name;
using Passport.Infrastructure.TypeHandler;

namespace Passport.Infrastructure.Persistence
{
    internal sealed class PassportHolderRepository : IPassportHolderRepository
    {
        private readonly IDataAccess sqlDataAccess;
        private readonly IPassportSetting ppSetting;

        public PassportHolderRepository([FromKeyedServices(DefaultKeyedServiceName.DataAccess)] IDataAccess sqlDataAccess, IPassportSetting ppSetting)
        {
            this.sqlDataAccess = sqlDataAccess;
            this.ppSetting = ppSetting;

            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> DeleteAsync(
            PassportHolderTransferObject dtoPassportHolder,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"DELETE FROM [{PassportHolderTable.PassportHolder}] 
									WHERE [{PassportHolderColumn.ConcurrencyStamp}] = @ConcurrencyStamp 
									AND [{PassportHolderColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ConcurrencyStamp", dtoPassportHolder.ConcurrencyStamp);
                dpParameter.Add("Id", dtoPassportHolder.Id);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not delete holder {dtoPassportHolder.Id}."
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
        public async Task<RepositoryResult<bool>> ExistsAsync(
            Guid guHolderId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT CASE WHEN EXISTS(
									SELECT 1 FROM [{PassportHolderTable.PassportHolder}] 
									WHERE [{PassportHolderColumn.Id}] = @Id) 
									THEN 1 ELSE 0 END;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", guHolderId);

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
        public async Task<RepositoryResult<PassportHolderTransferObject>> FindByIdAsync(
            Guid guHolderId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<PassportHolderTransferObject>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT 
									[{PassportHolderColumn.ConcurrencyStamp}], 
									[{PassportHolderColumn.CultureName}], 
									[{PassportHolderColumn.EmailAddress}], 
									[{PassportHolderColumn.EmailAddressIsConfirmed}], 
									[{PassportHolderColumn.FirstName}], 
									[{PassportHolderColumn.Id}], 
									[{PassportHolderColumn.LastName}], 
									[{PassportHolderColumn.PhoneNumber}], 
									[{PassportHolderColumn.PhoneNumberIsConfirmed}], 
									[{PassportHolderColumn.SecurityStamp}] 
									FROM [{PassportHolderTable.PassportHolder}] 
									WHERE [{PassportHolderColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", guHolderId);

                IEnumerable<PassportHolderTransferObject> enumPassportHolderTransferObject = await sqlDataAccess.Connection.QueryAsync<PassportHolderTransferObject>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (enumPassportHolderTransferObject.Count() == 0)
                    return new RepositoryResult<PassportHolderTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"No data for {guHolderId} has been found."
                    });

                return new RepositoryResult<PassportHolderTransferObject>(enumPassportHolderTransferObject.First());
            }
            catch (Exception exException)
            {
                return new RepositoryResult<PassportHolderTransferObject>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> InsertAsync(
            PassportHolderTransferObject dtoPassportHolder,
            DateTimeOffset dtCreatedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"INSERT INTO [{PassportHolderTable.PassportHolder}](
									[{PassportHolderColumn.ConcurrencyStamp}], 
									[{PassportHolderColumn.CreatedAt}], 
									[{PassportHolderColumn.CultureName}], 
									[{PassportHolderColumn.EditedAt}], 
									[{PassportHolderColumn.EmailAddress}], 
									[{PassportHolderColumn.EmailAddressIsConfirmed}], 
									[{PassportHolderColumn.FirstName}], 
									[{PassportHolderColumn.Id}], 
									[{PassportHolderColumn.LastName}], 
									[{PassportHolderColumn.PhoneNumber}], 
									[{PassportHolderColumn.PhoneNumberIsConfirmed}], 
									[{PassportHolderColumn.SecurityStamp}]) 
									SELECT 
									@ConcurrencyStamp, 
									@CreatedAt, 
									@CultureName, 
									@EditedAt, 
									@EmailAddress, 
									@EmailAddressIsConfirmed, 
									@FirstName, 
									@Id, 
									@LastName, 
									@PhoneNumber, 
									@PhoneNumberIsConfirmed, 
									@SecurityStamp 
									WHERE NOT EXISTS (
									SELECT 1 
									FROM [{PassportHolderTable.PassportHolder}] 
									WHERE [{PassportHolderColumn.EmailAddress}] = @EmailAddress 
									OR [{PassportHolderColumn.Id}] = @Id);";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ConcurrencyStamp", dtoPassportHolder.ConcurrencyStamp);
                dpParameter.Add("CreatedAt", dtCreatedAt);
                dpParameter.Add("CultureName", dtoPassportHolder.CultureName);
                dpParameter.Add("EditedAt", dtCreatedAt);
                dpParameter.Add("EmailAddress", dtoPassportHolder.EmailAddress);
                dpParameter.Add("EmailAddressIsConfirmed", dtoPassportHolder.EmailAddressIsConfirmed);
                dpParameter.Add("FirstName", dtoPassportHolder.FirstName);
                dpParameter.Add("Id", dtoPassportHolder.Id);
                dpParameter.Add("LastName", dtoPassportHolder.LastName);
                dpParameter.Add("PhoneNumber", dtoPassportHolder.PhoneNumber);
                dpParameter.Add("PhoneNumberIsConfirmed", dtoPassportHolder.PhoneNumberIsConfirmed);
                dpParameter.Add("SecurityStamp", dtoPassportHolder.SecurityStamp);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not create holder {dtoPassportHolder.Id}."
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
        public async Task<RepositoryResult<bool>> UpdateAsync(
            PassportHolderTransferObject dtoPassportHolder,
            DateTimeOffset dtEditedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportHolderTable.PassportHolder}] SET 
									[{PassportHolderColumn.ConcurrencyStamp}] = @ConcurrencyStamp, 
									[{PassportHolderColumn.CultureName}] = @CultureName, 
									[{PassportHolderColumn.EditedAt}] = @EditedAt, 
									[{PassportHolderColumn.EmailAddress}] = @EmailAddress, 
									[{PassportHolderColumn.EmailAddressIsConfirmed}] = @EmailAddressIsConfirmed, 
									[{PassportHolderColumn.FirstName}] = @FirstName, 
									[{PassportHolderColumn.Id}] = @Id, 
									[{PassportHolderColumn.LastName}] = @LastName, 
									[{PassportHolderColumn.PhoneNumber}] = @PhoneNumber, 
									[{PassportHolderColumn.PhoneNumberIsConfirmed}] = @PhoneNumberIsConfirmed, 
									[{PassportHolderColumn.SecurityStamp}] = @SecurityStamp 
									WHERE [{PassportHolderColumn.ConcurrencyStamp}] = @ActualConcurrencyStamp 
									AND [{PassportHolderColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ActualConcurrencyStamp", dtoPassportHolder.ConcurrencyStamp);
                dpParameter.Add("ConcurrencyStamp", Guid.NewGuid());
                dpParameter.Add("CultureName", dtoPassportHolder.CultureName);
                dpParameter.Add("EditedAt", dtEditedAt);
                dpParameter.Add("EmailAddress", dtoPassportHolder.EmailAddress);
                dpParameter.Add("EmailAddressIsConfirmed", dtoPassportHolder.EmailAddressIsConfirmed);
                dpParameter.Add("FirstName", dtoPassportHolder.FirstName);
                dpParameter.Add("Id", dtoPassportHolder.Id);
                dpParameter.Add("LastName", dtoPassportHolder.LastName);
                dpParameter.Add("PhoneNumber", dtoPassportHolder.PhoneNumber);
                dpParameter.Add("PhoneNumberIsConfirmed", dtoPassportHolder.PhoneNumberIsConfirmed);
                dpParameter.Add("SecurityStamp", dtoPassportHolder.SecurityStamp);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not update holder {dtoPassportHolder.Id}."
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
    }
}
