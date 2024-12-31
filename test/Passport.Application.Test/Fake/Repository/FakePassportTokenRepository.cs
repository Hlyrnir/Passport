using Passport.Abstraction.Authentication;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

#pragma warning disable CS1998

namespace Passport.Application.Test.Fake.Repository
{
    internal sealed class FakePassportTokenRepository : IPassportTokenRepository
    {
        private readonly IPassportSetting ppSetting;

        private IDictionary<Guid, PassportTokenTransferObject> dictToken;
        private IDictionary<Guid, IPassportCredential> dictCredential;
        private IDictionary<Guid, int> dictFailedAttemptCounter;

        public FakePassportTokenRepository(FakeDatabase dbFake, IPassportSetting ppSetting)
        {
            this.ppSetting = ppSetting;

            dictToken = dbFake.Token;
            dictCredential = dbFake.Credential;
            dictFailedAttemptCounter = dbFake.FailedAttemptCounter;
        }


        public async Task<RepositoryResult<bool>> CredentialAtProviderExistsAsync(string sCredential, string sProvider, CancellationToken tknCancellation)
        {
            foreach (KeyValuePair<Guid, IPassportCredential> kvpCredential in dictCredential)
            {
                if (kvpCredential.Value.Credential == sCredential
                    && kvpCredential.Value.Provider == sProvider)
                    return new RepositoryResult<bool>(true);
            }

            return new RepositoryResult<bool>(false);
        }

        public async Task<RepositoryResult<bool>> DeleteAsync(PassportTokenTransferObject dtoToken, CancellationToken tknCancellation)
        {
            if (dictToken.ContainsKey(dtoToken.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.NotFound);

            if (dictCredential.Remove(dtoToken.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.Credential.NotFound);

            if (dictFailedAttemptCounter.Remove(dtoToken.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.FailedAttemptCounter.NotFound);

            if (dictToken.Remove(dtoToken.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.NotFound);

            return new RepositoryResult<bool>(true);
        }

        public async Task<RepositoryResult<bool>> EnableTwoFactorAuthenticationAsync(PassportTokenTransferObject dtoToken, bool bIsEnabled, DateTimeOffset dtEditedAt, CancellationToken tknCancellation)
        {
            if (dictToken.TryGetValue(dtoToken.Id, out PassportTokenTransferObject? dtoTokenInDictionary) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.NotFound);

            Passport.Domain.Aggregate.PassportToken? ppToken = dtoTokenInDictionary.Initialize();

            if (ppToken is null)
                return new RepositoryResult<bool>(TestError.Repository.Domain.NotInitialized);

            bool bResult = ppToken.TryEnableTwoFactorAuthentication(bIsEnabled);

            if (bResult == true)
                dictToken[dtoToken.Id] = ppToken.MapToTransferObject();

            return new RepositoryResult<bool>(bResult);
        }

        public async Task<RepositoryResult<PassportTokenTransferObject>> FindTokenByCredentialAsync(IPassportCredential ppCredential, DateTimeOffset dtAttemptedAt, CancellationToken tknCancellation)
        {
            foreach (KeyValuePair<Guid, IPassportCredential> kvpCredential in dictCredential)
            {
                if (ppCredential.Credential == kvpCredential.Value.Credential
                    && ppCredential.Provider == kvpCredential.Value.Provider
                    && ppCredential.Signature == kvpCredential.Value.Signature)
                {
                    if (dictToken.TryGetValue(kvpCredential.Key, out PassportTokenTransferObject? dtoTokenInDictionary) == false)
                        return new RepositoryResult<PassportTokenTransferObject>(TestError.Repository.PassportToken.NotFound);

                    if (dictFailedAttemptCounter.TryGetValue(kvpCredential.Key, out _) == false)
                        return new RepositoryResult<PassportTokenTransferObject>(TestError.Repository.PassportToken.FailedAttemptCounter.NotFound);

                    PassportTokenTransferObject dtoPassportToken = dtoTokenInDictionary.Clone(dtAttemptedAt.Add(ppSetting.RefreshTokenExpiresAfterDuration));

                    dictFailedAttemptCounter[kvpCredential.Key] = 0;
                    dictToken[kvpCredential.Key] = dtoPassportToken;

                    return new RepositoryResult<PassportTokenTransferObject>(dtoPassportToken);
                }
            }

            return new RepositoryResult<PassportTokenTransferObject>(TestError.Repository.PassportToken.Credential.NotFound);
        }

        public async Task<RepositoryResult<PassportTokenTransferObject>> FindTokenByRefreshTokenAsync(Guid guPassportId, string sProvider, string sRefreshToken, DateTimeOffset dtAttemptedAt, CancellationToken tknCancellation)
        {
            Guid guTokenId = Guid.Empty;

            foreach (KeyValuePair<Guid, PassportTokenTransferObject> kvpToken in dictToken)
            {
                if (kvpToken.Value.PassportId == guPassportId)
                    guTokenId = kvpToken.Key;
            }

            if (dictToken.TryGetValue(guTokenId, out PassportTokenTransferObject? dtoTokenInDictionary) == false)
                return new RepositoryResult<PassportTokenTransferObject>(TestError.Repository.PassportToken.NotFound);

            if (dtoTokenInDictionary.ExpiredAt > dtAttemptedAt
                && dtoTokenInDictionary.Provider == sProvider
                && dtoTokenInDictionary.RefreshToken == sRefreshToken)
            {
                if (dictFailedAttemptCounter.TryGetValue(guTokenId, out _) == false)
                    return new RepositoryResult<PassportTokenTransferObject>(TestError.Repository.PassportToken.FailedAttemptCounter.NotFound);

                PassportTokenTransferObject dtoPassportToken = dtoTokenInDictionary.Clone(dtAttemptedAt.Add(ppSetting.RefreshTokenExpiresAfterDuration));

                dictFailedAttemptCounter[guTokenId] = 0;
                dictToken[guTokenId] = dtoPassportToken;

                return new RepositoryResult<PassportTokenTransferObject>(dtoPassportToken);
            }

            return new RepositoryResult<PassportTokenTransferObject>(TestError.Repository.PassportToken.RefreshToken.NotFound);
        }

        public async Task<RepositoryResult<bool>> InsertAsync(PassportTokenTransferObject dtoToken, IPassportCredential ppCredential, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation)
        {
            if (dictToken.ContainsKey(dtoToken.Id) == true)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.Exists);

            if (dictCredential.TryAdd(dtoToken.Id, ppCredential) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.Credential.Exists);

            if (dictFailedAttemptCounter.TryAdd(dtoToken.Id, 0) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.FailedAttemptCounter.NotAdded);

            if (dictToken.TryAdd(dtoToken.Id, dtoToken) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.Exists);

            return new RepositoryResult<bool>(true);
        }

        public async Task<RepositoryResult<bool>> ResetCredentialAsync(PassportTokenTransferObject dtoToken, IPassportCredential ppCredentialToApply, DateTimeOffset dtResetAt, CancellationToken tknCancellation)
        {
            if (dictToken.ContainsKey(dtoToken.Id) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.NotFound);

            dictCredential[dtoToken.Id] = DataFaker.PassportCredential.Create(ppCredentialToApply.Credential, ppCredentialToApply.Signature);

            return new RepositoryResult<bool>(true);
        }

        public async Task<RepositoryResult<bool>> ResetRefreshTokenAsync(Guid guPassportId, string sProvider, DateTimeOffset dtResetAt, CancellationToken tknCancellation)
        {
            Guid guTokenId = Guid.Empty;

            foreach (KeyValuePair<Guid, PassportTokenTransferObject> kvpToken in dictToken)
            {
                if (kvpToken.Value.PassportId == guPassportId)
                    guTokenId = kvpToken.Key;
            }

            if (dictToken.TryGetValue(guTokenId, out PassportTokenTransferObject? dtoTokenInDictionary) == false)
                return new RepositoryResult<bool>(TestError.Repository.PassportToken.NotFound);

            PassportTokenTransferObject dtoPassportToken = dtoTokenInDictionary.Clone(dtResetAt.Add(ppSetting.RefreshTokenExpiresAfterDuration));

            dictToken[guTokenId] = dtoPassportToken;

            return new RepositoryResult<bool>(true);
        }

        public async Task<RepositoryResult<int>> VerifyCredentialAsync(IPassportCredential ppCredential, DateTimeOffset dtVerifiedAt, CancellationToken tknCancellation)
        {
            int iActualCount = ppSetting.MaximalAllowedAccessAttempt;

            foreach (KeyValuePair<Guid, IPassportCredential> kvpCredential in dictCredential)
            {
                if (dictFailedAttemptCounter.TryGetValue(kvpCredential.Key, out iActualCount) == false)
                    return new RepositoryResult<int>(TestError.Repository.PassportToken.FailedAttemptCounter.NotFound);

                if (kvpCredential.Value.Credential == ppCredential.Credential
                    && kvpCredential.Value.Provider == ppCredential.Provider
                    && kvpCredential.Value.Signature != ppCredential.Signature)
                {
                    if (iActualCount < ppSetting.MaximalAllowedAccessAttempt)
                    {
                        iActualCount++;
                        dictFailedAttemptCounter[kvpCredential.Key] = iActualCount;
                    }
                }
            }

            return new RepositoryResult<int>(ppSetting.MaximalAllowedAccessAttempt + -1 * iActualCount);
        }

        public async Task<RepositoryResult<int>> VerifyRefreshTokenAsync(Guid guPassportId, string sProvider, string sRefreshToken, DateTimeOffset dtVerifiedAt, CancellationToken tknCancellation)
        {
            int iActualCount = ppSetting.MaximalAllowedAccessAttempt;

            Guid guTokenId = Guid.Empty;

            foreach (KeyValuePair<Guid, PassportTokenTransferObject> kvpToken in dictToken)
            {
                if (kvpToken.Value.PassportId == guPassportId)
                    guTokenId = kvpToken.Key;
            }

            if (dictFailedAttemptCounter.TryGetValue(guTokenId, out iActualCount) == false)
                return new RepositoryResult<int>(TestError.Repository.PassportToken.FailedAttemptCounter.NotFound);

            if (dictToken.TryGetValue(guTokenId, out PassportTokenTransferObject? dtoTokenInDictionary) == false)
                return new RepositoryResult<int>(TestError.Repository.PassportToken.NotFound);

            if (dtoTokenInDictionary.Provider == sProvider
                    && (dtoTokenInDictionary.ExpiredAt < dtVerifiedAt
                    || dtoTokenInDictionary.RefreshToken != sRefreshToken))
            {
                if (iActualCount < ppSetting.MaximalAllowedAccessAttempt)
                {
                    iActualCount++;
                    dictFailedAttemptCounter[guTokenId] = iActualCount;
                }
            }

            return new RepositoryResult<int>(ppSetting.MaximalAllowedAccessAttempt + -1 * iActualCount);
        }
    }
}

#pragma warning restore CS1998