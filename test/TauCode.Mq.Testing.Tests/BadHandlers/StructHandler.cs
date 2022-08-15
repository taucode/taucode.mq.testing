using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers;

public struct StructHandler : IMessageHandler<HelloMessage>
{
    public Task HandleAsync(HelloMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task HandleAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}