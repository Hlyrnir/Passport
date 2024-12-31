using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

#pragma warning disable CS1998

namespace Passport.Application.Test.Fake.Repository
{
    internal sealed class FakePassportHolderRepository : IPassportHolderRepository
    {
        private readonly IPassportSetting ppSetting;

        private IDictionary<Guid, PassportHolderTransferObject> dictHolder;

        public FakePassportHolderRepository(FakeDatabase dbFake, IPassportSetting ppSetting)
        {
            this.ppSetting = ppSetting;

            dictHolder = dbFake.Holder;
        }


        public async Task<RepositoryResult<bool>> DeleteAsync(PassportHolderTransferObject dtoHolder, CancellationToken tknCancellation)
        {
            if (dictHolder.ContainsKey(dtoHolder.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportHolder.NotFound);

            return new RepositoryResult<bool>(dictHolder.Remove(dtoHolder.Id));
        }

        public async Task<RepositoryResult<bool>> ExistsAsync(Guid guHolderId, CancellationToken tknCancellation)
        {
            return new RepositoryResult<bool>(dictHolder.ContainsKey(guHolderId));
        }

        public async Task<RepositoryResult<PassportHolderTransferObject>> FindByIdAsync(Guid guHolderId, CancellationToken tknCancellation)
        {
            dictHolder.TryGetValue(guHolderId, out PassportHolderTransferObject? dtoHolderInRepository);

            if (dtoHolderInRepository is null)
                return new RepositoryResult<PassportHolderTransferObject>(TestError.Repository.PassportHolder.NotFound);

            return new RepositoryResult<PassportHolderTransferObject>(dtoHolderInRepository.Clone());
        }

        public async Task<RepositoryResult<bool>> InsertAsync(PassportHolderTransferObject dtoHolder, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation)
        {
            if (dictHolder.ContainsKey(dtoHolder.Id) == true)
                return new RepositoryResult<bool>(TestError.Repository.PassportHolder.Exists);

            bool bResult = dictHolder.TryAdd(dtoHolder.Id, dtoHolder);

            return new RepositoryResult<bool>(bResult);
        }

        public async Task<RepositoryResult<bool>> UpdateAsync(PassportHolderTransferObject dtoHolder, DateTimeOffset dtEditedAt, CancellationToken tknCancellation)
        {
            if (dictHolder.ContainsKey(dtoHolder.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportHolder.NotFound);

            dictHolder[dtoHolder.Id] = dtoHolder.Clone();

            return new RepositoryResult<bool>(true);
        }
    }
}

#pragma warning restore CS1998