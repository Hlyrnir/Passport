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
    internal sealed class PassportVisaRepository : IPassportVisaRepository
    {
        private readonly IDataAccess sqlDataAccess;

        public PassportVisaRepository([FromKeyedServices(DefaultKeyedServiceName.DataAccess)] IDataAccess sqlDataAccess)
        {
            this.sqlDataAccess = sqlDataAccess;

            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> DeleteAsync(
            PassportVisaTransferObject dtoPassportVisa,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"DELETE FROM [{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.ConcurrencyStamp}] = @ConcurrencyStamp 
									AND [{PassportVisaColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ConcurrencyStamp", dtoPassportVisa.ConcurrencyStamp);
                dpParameter.Add("Id", dtoPassportVisa.Id);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not delete visa {dtoPassportVisa.Id}."
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
            Guid guVisaId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT CASE WHEN EXISTS(
									SELECT 1 FROM [{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.Id}] = @Id) 
									THEN 1 ELSE 0 END;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", guVisaId);

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
        public async Task<RepositoryResult<bool>> ExistsAsync(
            IEnumerable<Guid> enumVisaId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT COUNT(*) 
									FROM [{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.Id}] IN @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", enumVisaId);

                int iResult = 0;

                iResult = await sqlDataAccess.Connection.ExecuteScalarAsync<int>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                return new RepositoryResult<bool>(iResult == enumVisaId.Count());
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
        public async Task<RepositoryResult<bool>> ByNameAtLevelExistsAsync(
            string sName,
            int iLevel,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT CASE WHEN EXISTS(
									SELECT 1 FROM [{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.Name}] = @Name 
									AND [{PassportVisaColumn.Level}] = @Level) 
									THEN 1 ELSE 0 END;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Name", sName);
                dpParameter.Add("Level", iLevel);

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
        public async Task<RepositoryResult<PassportVisaTransferObject>> FindByIdAsync(
            Guid guVisaId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<PassportVisaTransferObject>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT 
									[{PassportVisaColumn.ConcurrencyStamp}],
									[{PassportVisaColumn.Id}],
									[{PassportVisaColumn.Name}],
									[{PassportVisaColumn.Level}] 
									FROM [{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.Id}] = @Id;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Id", guVisaId);

                IEnumerable<PassportVisaTransferObject> enumPassportTransferObject = await sqlDataAccess.Connection.QueryAsync<PassportVisaTransferObject>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (enumPassportTransferObject.Count() == 0)
                    return new RepositoryResult<PassportVisaTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"No data for {guVisaId} has been found."
                    });

                return new RepositoryResult<PassportVisaTransferObject>(enumPassportTransferObject.First());
            }
            catch (Exception exException)
            {
                return new RepositoryResult<PassportVisaTransferObject>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<PassportVisaTransferObject>> FindByNameAsync(
            string sName,
            int iLevel,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<PassportVisaTransferObject>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT 
									[{PassportVisaColumn.ConcurrencyStamp}],
									[{PassportVisaColumn.Id}],
									[{PassportVisaColumn.Name}],
									[{PassportVisaColumn.Level}] 
									FROM [{PassportVisaTable.PassportVisa}] 
									WHERE [{PassportVisaColumn.Name}] = @Name  
									AND [{PassportVisaColumn.Level}] = @Level;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("Name", sName);
                dpParameter.Add("Level", iLevel);

                IEnumerable<PassportVisaTransferObject> enumPassportTransferObject = await sqlDataAccess.Connection.QueryAsync<PassportVisaTransferObject>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (enumPassportTransferObject.Count() == 0)
                    return new RepositoryResult<PassportVisaTransferObject>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"No data for visa of name {sName} at level {iLevel} has been found."
                    });

                return new RepositoryResult<PassportVisaTransferObject>(enumPassportTransferObject.First());
            }
            catch (Exception exException)
            {
                return new RepositoryResult<PassportVisaTransferObject>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<IEnumerable<PassportVisaTransferObject>>> FindByPassportAsync(
            Guid guPassportId,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<IEnumerable<PassportVisaTransferObject>>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"SELECT 
									A.[{PassportVisaColumn.ConcurrencyStamp}],
									A.[{PassportVisaColumn.Id}],
									A.[{PassportVisaColumn.Name}],
									A.[{PassportVisaColumn.Level}] 
									FROM [{PassportVisaTable.PassportVisa}] AS A 
									INNER JOIN [{PassportVisaRegisterTable.PassportVisaRegister}] 
									ON [{PassportVisaRegisterTable.PassportVisaRegister}].[{PassportVisaRegisterColumn.PassportVisaId}] = A.[{PassportVisaColumn.Id}] 
									INNER JOIN [{PassportTable.Passport}] AS B 
									ON [{PassportVisaRegisterTable.PassportVisaRegister}].[{PassportVisaRegisterColumn.PassportId}] = B.[{PassportColumn.Id}] 
									WHERE B.[{PassportColumn.Id}] = @PassportId;";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("PassportId", guPassportId);

                IEnumerable<PassportVisaTransferObject> enumPassportTransferObject = await sqlDataAccess.Connection.QueryAsync<PassportVisaTransferObject>(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                return new RepositoryResult<IEnumerable<PassportVisaTransferObject>>(enumPassportTransferObject);
            }
            catch (Exception exException)
            {
                return new RepositoryResult<IEnumerable<PassportVisaTransferObject>>(new RepositoryError()
                {
                    Code = DefaultRepositoryError.Code.Exception,
                    Description = exException.Message
                });
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResult<bool>> InsertAsync(
            PassportVisaTransferObject dtoPassportVisa,
            DateTimeOffset dtCreatedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"INSERT INTO [{PassportVisaTable.PassportVisa}](
									[{PassportVisaColumn.ConcurrencyStamp}], 
									[{PassportVisaColumn.CreatedAt}], 
									[{PassportVisaColumn.EditedAt}], 
									[{PassportVisaColumn.Id}], 
									[{PassportVisaColumn.Name}], 
									[{PassportVisaColumn.Level}]) 
									SELECT 
									@ConcurrencyStamp, 
									@CreatedAt, 
									@EditedAt, 
									@Id, 
									@Name, 
									@Level 
									WHERE NOT EXISTS (
									SELECT 1
									FROM [{PassportVisaTable.PassportVisa}] 
									WHERE (
									[{PassportVisaColumn.Id}] = @Id
									) OR (
									[{PassportVisaColumn.Name}] = @Name 
									AND [{PassportVisaColumn.Level}] = @Level));";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ConcurrencyStamp", dtoPassportVisa.ConcurrencyStamp);
                dpParameter.Add("CreatedAt", dtCreatedAt);
                dpParameter.Add("EditedAt", dtCreatedAt);
                dpParameter.Add("Id", dtoPassportVisa.Id);
                dpParameter.Add("Name", dtoPassportVisa.Name);
                dpParameter.Add("Level", dtoPassportVisa.Level);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                sql: sStatement,
                param: dpParameter,
                transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not create visa {dtoPassportVisa.Id}."
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
            PassportVisaTransferObject dtoPassportVisa,
            DateTimeOffset dtEditedAt,
            CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new RepositoryResult<bool>(DefaultRepositoryError.TaskAborted);

            try
            {
                string sStatement = @$"UPDATE [{PassportVisaTable.PassportVisa}] SET 
									[{PassportVisaColumn.ConcurrencyStamp}] = @ConcurrencyStamp, 
									[{PassportVisaColumn.EditedAt}] = @EditedAt, 
									[{PassportVisaColumn.Name}] = @Name, 
									[{PassportVisaColumn.Level}] = @Level 
									WHERE [{PassportVisaColumn.ConcurrencyStamp}] = @ActualStamp 
									AND [{PassportVisaColumn.Id}] = @Id";

                DynamicParameters dpParameter = new DynamicParameters();
                dpParameter.Add("ActualStamp", dtoPassportVisa.ConcurrencyStamp);
                dpParameter.Add("ConcurrencyStamp", Guid.NewGuid());
                dpParameter.Add("EditedAt", dtEditedAt);
                dpParameter.Add("Id", dtoPassportVisa.Id);
                dpParameter.Add("Name", dtoPassportVisa.Name);
                dpParameter.Add("Level", dtoPassportVisa.Level);

                int iResult = -1;

                iResult = await sqlDataAccess.Connection.ExecuteAsync(
                    sql: sStatement,
                    param: dpParameter,
                    transaction: sqlDataAccess.Transaction);

                if (iResult < 1)
                    return new RepositoryResult<bool>(new RepositoryError()
                    {
                        Code = DefaultRepositoryError.Code.Method,
                        Description = $"Could not update visa {dtoPassportVisa.Id}."
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