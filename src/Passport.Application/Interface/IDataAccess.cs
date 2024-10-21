using System.Data;

namespace Passport.Application.Interface
{
    public interface IDataAccess
    {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }
        Task TransactionAsync(Func<Task> MethodForTransaction);
        bool TryCommit();
        bool TryRollback();
    }
}
