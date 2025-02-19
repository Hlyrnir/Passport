using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Interface
{
    public interface IPassportRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassport"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> DeleteAsync(PassportTransferObject dtoPassport, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guPassportId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> ExistsAsync(Guid guPassportId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guPassportId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<PassportTransferObject>> FindByIdAsync(Guid guPassportId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassport"></param>
        /// <param name="dtCreatedAt"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> InsertAsync(PassportTransferObject dtoPassport, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassport"></param>
        /// <param name="dtEditedAt"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> UpdateAsync(PassportTransferObject dtoPassport, DateTimeOffset dtEditedAt, CancellationToken tknCancellation);
    }
}
