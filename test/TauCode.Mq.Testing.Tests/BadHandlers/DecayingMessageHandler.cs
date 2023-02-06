using Serilog;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers;

public class DecayingMessageHandler : MessageHandlerBase<DecayingMessage>
{
    protected override Task HandleAsyncImpl(DecayingMessage message, CancellationToken cancellationToken = default)
    {
        Log.Information($"Decayed sync, {message.DecayedProperty}!");
        MessageRepository.Instance.Add(message);

        return Task.CompletedTask;
    }
}