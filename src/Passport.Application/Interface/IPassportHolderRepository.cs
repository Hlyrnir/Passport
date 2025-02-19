using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Interface
{
    public interface IPassportHolderRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassportHolder"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> DeleteAsync(PassportHolderTransferObject dtoPassportHolder, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guHolderId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> ExistsAsync(Guid guHolderId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guHolderId"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<PassportHolderTransferObject>> FindByIdAsync(Guid guHolderId, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassportHolder"></param>
        /// <param name="dtCreatedAt"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> InsertAsync(PassportHolderTransferObject dtoPassportHolder, DateTimeOffset dtCreatedAt, CancellationToken tknCancellation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtoPassportHolder"></param>
        /// <param name="dtEditedAt"></param>
        /// <param name="tknCancellation"></param>
        /// <returns></returns>
        Task<RepositoryResult<bool>> UpdateAsync(PassportHolderTransferObject dtoPassportHolder, DateTimeOffset dtEditedAt, CancellationToken tknCancellation);
    }
}
