using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

namespace Passport.Application.Test.Fake.Repository
{
    internal sealed class FakePassportRepository : IPassportRepository
    {
        private IDictionary<Guid, PassportTransferObject> dictPassport;
        private IDictionary<Guid, PassportVisaTransferObject> dictVisa;

        private IDictionary<Guid, IList<Guid>> dictVisaRegister;

        public FakePassportRepository(FakeDatabase dbFake)
        {
            dictPassport = dbFake.Passport;
            dictVisa = dbFake.Visa;
            dictVisaRegister = dbFake.VisaRegister;
        }

        public async Task<RepositoryResult<bool>> DeleteAsync(PassportTransferObject dtoPassport, CancellationToken tknCancellation)
        {
            if (dictPassport.ContainsKey(dtoPassport.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.Passport.NotFound);

            return new RepositoryResult<bool>(dictPassport.Remove(dtoPassport.Id));
        }

        public async Task<RepositoryResult<bool>> ExistsAsync(Guid guPassportId, CancellationToken tknCancellation)
        {
            return new RepositoryResult<bool>(dictPassport.ContainsKey(guPassportId));
        }

        public async Task<RepositoryResult<PassportTransferObject>> FindByIdAsync(Guid guPassportId, CancellationToken tknCancellation)
        {
            dictPassport.TryGetValue(guPassportId, out PassportTransferObject? dtoPassportInRepository);

            if (dtoPassportInRepository is null)
                return new RepositoryResult<PassportTransferObject>(TestError.Repository.Passport.NotFound);

            return new RepositoryResult<PassportTransferObject>(dtoPassportInRepository.Clone());
        }

        public async Task<RepositoryResult<bool>> InsertAsync(PassportTransferObject dtoPassport, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation)
        {
            if (dictPassport.ContainsKey(dtoPassport.Id) == true)
                return new RepositoryResult<bool>(TestError.Repository.Passport.Exists);

            bool bResult = dictPassport.TryAdd(dtoPassport.Id, dtoPassport);

            if (dictPassport.Keys.Contains(dtoPassport.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.Passport.VisaRegister);

            if (TryRegisterVisa(dtoPassport.Id, dtoPassport.VisaId) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportVisa.VisaRegister);

            return new RepositoryResult<bool>(bResult);
        }

        public async Task<RepositoryResult<bool>> UpdateAsync(PassportTransferObject dtoPassport, DateTimeOffset dtEditedAt, CancellationToken tknCancellation)
        {
            if (dictPassport.ContainsKey(dtoPassport.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.Passport.NotFound);

            if (TryRegisterVisa(dtoPassport.Id, dtoPassport.VisaId) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportVisa.NotFound);

            dictPassport[dtoPassport.Id] = dtoPassport.Clone();

            return new RepositoryResult<bool>(true);
        }

        private bool TryRegisterVisa(Guid guPassportId, IEnumerable<Guid> enumVisaId)
        {
            IList<Guid> lstVisa = new List<Guid>();

            foreach (Guid ppVisaId in enumVisaId)
            {
                if (dictVisa.Keys.Contains(ppVisaId) == false)
                    return false;

                lstVisa.Add(ppVisaId);
            }

            if (dictVisaRegister.ContainsKey(guPassportId) == true)
                dictVisaRegister[guPassportId] = lstVisa;
            else
                dictVisaRegister.Add(guPassportId, lstVisa);

            return true;
        }
    }
}

#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

