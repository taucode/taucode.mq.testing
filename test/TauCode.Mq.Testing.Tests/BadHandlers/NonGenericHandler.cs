namespace TauCode.Mq.Testing.Tests.BadHandlers;

public class NonGenericHandler : IMessageHandler
{
    public Task HandleAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}