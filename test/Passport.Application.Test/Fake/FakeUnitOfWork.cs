using Passport.Application.Interface;

namespace Passport.Application.Test.Fake
{
    internal class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork()
        {

        }

        public async Task TransactionAsync(Func<Task> MethodForTransaction)
        {
            await MethodForTransaction();
        }

        public bool TryCommit()
        {
            return true;
        }

        public bool TryRollback()
        {
            return true;
        }
    }
}
