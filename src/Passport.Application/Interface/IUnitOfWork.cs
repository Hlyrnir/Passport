using System;
using System.Threading.Tasks;

namespace Passport.Application.Interface
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MethodForTransaction"></param>
        /// <returns></returns>
        Task TransactionAsync(Func<Task> MethodForTransaction);

        /// <summary>
        /// Commit the database transaction.
        /// </summary>
        /// <returns>Returns <see cref="bool">true</see> if the database transaction is successfully committed. Otherwise, returns <see cref="bool">false</see>.</returns>
        bool TryCommit();

        /// <summary>
        /// Roll back the database transaction.
        /// </summary>
        /// <returns>Returns <see cref="bool">true</see> if the database transaction is successfully rolled back. Otherwise, returns <see cref="bool">false</see>.</returns>
        bool TryRollback();
    }
}
