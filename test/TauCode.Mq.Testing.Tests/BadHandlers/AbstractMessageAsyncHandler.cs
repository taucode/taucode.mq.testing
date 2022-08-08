using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers;

public class AbstractMessageAsyncHandler : AsyncMessageHandlerBase<AbstractMessage>
{
    public override Task HandleAsync(AbstractMessage message, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}