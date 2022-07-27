using Serilog;
using TauCode.Mq.Abstractions;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public class DecayingMessageHandler : MessageHandlerBase<DecayingMessage>
    {
        public override void Handle(DecayingMessage message)
        {
            Log.Information($"Decayed sync, {message.DecayedProperty}!");
            MessageRepository.Instance.Add(message);
        }
    }
}
