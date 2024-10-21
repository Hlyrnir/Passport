using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Result;


#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

namespace Passport.Application.Test.Fake.Command
{
    internal class FakeCommandHandler : ICommandHandler<FakeCommand, IMessageResult<FakeResult>>
    {
        public async ValueTask<IMessageResult<FakeResult>> Handle(FakeCommand msgCommand, CancellationToken tknCancellation)
        {
            return new MessageResult<FakeResult>(new FakeResult());
        }
    }
}

#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

