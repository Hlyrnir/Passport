using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

#pragma warning disable CS1998

namespace Passport.Application.Test.Fake.Repository
{
    internal sealed class FakePassportVisaRepository : IPassportVisaRepository
    {
        private IDictionary<Guid, PassportVisaTransferObject> dictVisa;
        private IDictionary<Guid, IList<Guid>> dictVisaRegister;

        public FakePassportVisaRepository(FakeDatabase dbFake)
        {
            dictVisa = dbFake.Visa;
            dictVisaRegister = dbFake.VisaRegister;
        }

        public async Task<RepositoryResult<bool>> DeleteAsync(PassportVisaTransferObject dtoVisa, CancellationToken tknCancellation)
        {
            if (dictVisa.ContainsKey(dtoVisa.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportVisa.NotFound);

            return new RepositoryResult<bool>(dictVisa.Remove(dtoVisa.Id));
        }

        public async Task<RepositoryResult<bool>> ExistsAsync(Guid guVisaId, CancellationToken tknCancellation)
        {
            return new RepositoryResult<bool>(dictVisa.ContainsKey(guVisaId));
        }

        public async Task<RepositoryResult<bool>> ExistsAsync(IEnumerable<Guid> enumVisaId, CancellationToken tknCancellation)
        {
            foreach (Guid guPassportVisaId in enumVisaId)
            {
                if (dictVisa.ContainsKey(guPassportVisaId) == false)
                    return new RepositoryResult<bool>(false);
            }

            return new RepositoryResult<bool>(true);
        }

        public async Task<RepositoryResult<bool>> ByNameAtLevelExistsAsync(string sName, int iLevel, CancellationToken tknCancellation)
        {
            foreach (PassportVisaTransferObject dtoPassportVisa in dictVisa.Values)
            {
                if (dtoPassportVisa.Name == sName && dtoPassportVisa.Level == iLevel)
                    return new RepositoryResult<bool>(true);
            }

            return new RepositoryResult<bool>(false);
        }

        public async Task<RepositoryResult<PassportVisaTransferObject>> FindByIdAsync(Guid guVisaId, CancellationToken tknCancellation)
        {
            dictVisa.TryGetValue(guVisaId, out PassportVisaTransferObject? ppVisaInRepository);

            if (ppVisaInRepository is null)
                return new RepositoryResult<PassportVisaTransferObject>(TestError.Repository.PassportVisa.NotFound);

            return new RepositoryResult<PassportVisaTransferObject>(ppVisaInRepository.Clone());
        }

        public async Task<RepositoryResult<PassportVisaTransferObject>> FindByNameAsync(string sName, int iLevel, CancellationToken tknCancellation)
        {
            foreach (KeyValuePair<Guid, PassportVisaTransferObject> kvpVisa in dictVisa)
            {
                if (kvpVisa.Value.Name == sName
                    && kvpVisa.Value.Level == iLevel)
                {
                    return new RepositoryResult<PassportVisaTransferObject>(kvpVisa.Value.Clone());
                }
            }

            return new RepositoryResult<PassportVisaTransferObject>(TestError.Repository.PassportVisa.NotFound);
        }

        public async Task<RepositoryResult<IEnumerable<PassportVisaTransferObject>>> FindByPassportAsync(Guid guPassportId, CancellationToken tknCancellation)
        {
            IList<PassportVisaTransferObject> lstVisa = new List<PassportVisaTransferObject>();

            if (dictVisaRegister.TryGetValue(guPassportId, out IList<Guid>? lstVisaId) == false)
                return new RepositoryResult<IEnumerable<PassportVisaTransferObject>>(TestError.Repository.PassportVisa.VisaRegister);

            foreach (Guid guVisaId in lstVisaId)
            {
                if (dictVisa.TryGetValue(guVisaId, out PassportVisaTransferObject? dtoVisaInRepository) == true)
                {
                    lstVisa.Add(dtoVisaInRepository.Clone());
                }
            }

            return new RepositoryResult<IEnumerable<PassportVisaTransferObject>>(lstVisa.AsEnumerable());
        }

        public async Task<RepositoryResult<bool>> InsertAsync(PassportVisaTransferObject dtoVisa, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation)
        {
            if (dictVisa.ContainsKey(dtoVisa.Id) == true)
                return new RepositoryResult<bool>(TestError.Repository.PassportVisa.Exists);

            bool bResult = dictVisa.TryAdd(dtoVisa.Id, dtoVisa);

            return new RepositoryResult<bool>(bResult);
        }

        public async Task<RepositoryResult<bool>> UpdateAsync(PassportVisaTransferObject dtoVisa, DateTimeOffset dtEditedAt, CancellationToken tknCancellation)
        {
            if (dictVisa.ContainsKey(dtoVisa.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportVisa.NotFound);

            dictVisa[dtoVisa.Id] = dtoVisa.Clone();

            return new RepositoryResult<bool>(true);
        }
    }
}

#pragma warning restore CS1998
