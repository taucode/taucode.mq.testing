using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Mq.Abstractions;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public class BothSyncAndAsyncHandler : IMessageHandler<HelloMessage>, IAsyncMessageHandler<HelloMessage>
    {
        public void Handle(HelloMessage message)
        {
            throw new NotSupportedException();
        }

        public void Handle(object message)
        {
            throw new NotSupportedException();
        }

        public Task HandleAsync(HelloMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task HandleAsync(object message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
