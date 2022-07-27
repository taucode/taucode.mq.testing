using TauCode.Mq.Abstractions;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public abstract class AbstractHandler : MessageHandlerBase<HelloMessage>
    {
    }
}
