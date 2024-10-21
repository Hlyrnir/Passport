using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Passport.Application.Interface;
using System.Data;

namespace Passport.Infrastructure
{
    internal class SqliteDataAccess : IDataAccess, IDisposable
    {
        private IDbConnection sqlConnection;
        private IDbTransaction? sqlTransaction;

        public SqliteDataAccess(IConfiguration cfgConfiguration, string sConnectionStringName = "Default")
        {
            this.sqlConnection = new SqliteConnection(cfgConfiguration.GetConnectionString(sConnectionStringName));
            this.sqlTransaction = null;
        }

        public IDbConnection Connection { get => sqlConnection; }
        public IDbTransaction? Transaction { get => sqlTransaction; }

        public async Task TransactionAsync(Func<Task> MethodForTransaction)
        {
            if (MethodForTransaction is null)
                throw new NotImplementedException("Transaction method is not defined.");

            sqlConnection.Open();

            if (sqlTransaction is null)
                sqlTransaction = sqlConnection.BeginTransaction();

            await MethodForTransaction();

            sqlConnection.Close();
        }

        public bool TryCommit()
        {
            if (sqlTransaction is null)
                return false;

            sqlTransaction.Commit();
            sqlTransaction = null;

            return true;
        }

        public bool TryRollback()
        {
            if (sqlTransaction is null)
                return false;

            sqlTransaction.Rollback();
            sqlTransaction = null;

            return true;
        }

        #region Dispose
        // see https://learn.microsoft.com/en-us/dotnet/api/system.idisposable.dispose?view=net-8.0#system-idisposable-dispose
        // see https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose

        private bool bIsDisposed = false;

        public void Dispose()
        {
            // Dispose of managed and unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // In summary, managed resources are automatically managed by the .NET runtime environment,
        // while unmanaged resources are not. Developers must manage unmanaged resources manually
        // using the IDisposable interface to ensure that they are properly allocated and released.
        //
        // Dispose(bool bIsDisposing) executes in two distinct scenarios.
        // If bIsDisposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If bIsDisposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool bIsDisposing)
        //protected virtual void Dispose(bool bIsDisposing)
        {
            // Check to see if Dispose has already been called.
            if (!bIsDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (bIsDisposing)
                {
                    // Dispose managed resources

                    // ...
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                if (sqlTransaction is not null)
                    sqlTransaction.Dispose();

                sqlTransaction = null;

                if (sqlConnection is not null)
                    sqlConnection.Dispose();

                // Note disposing has been done.
                bIsDisposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        ~SqliteDataAccess() => Dispose(false);
        #endregion
    }
}
