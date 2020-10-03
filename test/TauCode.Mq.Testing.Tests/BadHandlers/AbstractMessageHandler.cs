using System;
using TauCode.Mq.Abstractions;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public class AbstractMessageHandler : MessageHandlerBase<AbstractMessage>
    {
        public override void Handle(AbstractMessage message)
        {
            throw new NotSupportedException();
        }
    }
}
