using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers;

public class StructMessageAsyncHandler : AsyncMessageHandlerBase<StructMessage>
{
    public override Task HandleAsync(StructMessage message, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}