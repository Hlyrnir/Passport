using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Interface
{
    public interface IPassportVisaRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassportVisa"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> DeleteAsync(PassportVisaTransferObject dtoPassportVisa, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guVisaId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> ExistsAsync(Guid guVisaId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumVisaId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> ExistsAsync(IEnumerable<Guid> enumVisaId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="iLevel"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> ByNameAtLevelExistsAsync(string sName, int iLevel, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guVisaId"></param>
        /// <param name="iLevel"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<PassportVisaTransferObject>> FindByIdAsync(Guid guVisaId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="iLevel"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<PassportVisaTransferObject>> FindByNameAsync(string sName, int iLevel, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guPassportId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<IEnumerable<PassportVisaTransferObject>>> FindByPassportAsync(Guid guPassportId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassportVisa"></param>
        /// <param name="dtCreatedAt"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> InsertAsync(PassportVisaTransferObject dtoPassportVisa, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassportVisa"></param>
        /// <param name="dtEditedAt"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> UpdateAsync(PassportVisaTransferObject dtoPassportVisa, DateTimeOffset dtEditedAt, CancellationToken tknCancellation);
    }
}
