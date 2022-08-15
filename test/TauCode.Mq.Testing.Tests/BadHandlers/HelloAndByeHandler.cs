using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers;

public class HelloAndByeHandler : IMessageHandler<HelloMessage>, IMessageHandler<ByeMessage>
{
    public Task HandleAsync(HelloMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task HandleAsync(ByeMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task HandleAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}