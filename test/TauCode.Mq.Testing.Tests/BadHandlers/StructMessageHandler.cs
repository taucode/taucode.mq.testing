using System;
using TauCode.Mq.Abstractions;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public class StructMessageHandler : MessageHandlerBase<StructMessage>
    {
        public override void Handle(StructMessage message)
        {
            throw new NotSupportedException();
        }
    }
}
