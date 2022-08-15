using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers;

public class AbstractMessageHandler : MessageHandlerBase<AbstractMessage>
{
    protected override Task HandleAsyncImpl(AbstractMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}