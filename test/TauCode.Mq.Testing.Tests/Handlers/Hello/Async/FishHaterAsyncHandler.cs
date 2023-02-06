using Serilog;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.Handlers.Hello.Async;

public class FishHaterAsyncHandler : MessageHandlerBase<HelloMessage>
{
    protected override async Task HandleAsyncImpl(HelloMessage message, CancellationToken cancellationToken = default)
    {
        var topicString = " (no topic)";
        if (message.Topic != null)
        {
            topicString = $" (topic: '{message.Topic}')";
        }

        if (message.Name.Contains("fish", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new Exception($"I hate you async{topicString}, '{message.Name}'! Exception thrown!");
        }

        await Task.Delay(message.MillisecondsTimeout, cancellationToken);

        Log.Information($"Not fish - then hi async{topicString}, {message.Name}!");
        MessageRepository.Instance.Add(message);
    }
}