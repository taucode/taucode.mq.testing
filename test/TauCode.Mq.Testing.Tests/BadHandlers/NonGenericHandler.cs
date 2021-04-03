using System;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public class NonGenericHandler : IMessageHandler
    {
        public void Handle(IMessage message)
        {
            throw new NotSupportedException();
        }
    }
}
