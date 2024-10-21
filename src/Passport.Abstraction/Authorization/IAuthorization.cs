using Mediator;
using Passport.Abstraction.Result;

namespace Passport.Abstraction.Authorization
{
    public interface IAuthorization<in T> where T : IMessage
    {
        public string PassportVisaName { get; }
        public int PassportVisaLevel { get; }

        public ValueTask<IMessageResult<bool>> AuthorizeAsync(T msgMessage, CancellationToken tknCancellation);
    }
}