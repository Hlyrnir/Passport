using Mediator;
using Passport.Abstraction.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Abstraction.Authorization
{
    public interface IAuthorization<in T> where T : IMessage
    {
        string PassportVisaName { get; }
        int PassportVisaLevel { get; }

        ValueTask<IMessageResult<bool>> AuthorizeAsync(T msgMessage, CancellationToken tknCancellation);
    }
}