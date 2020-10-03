using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Mq.Abstractions;
using TauCode.Mq.Testing.Tests.Messages;

namespace TauCode.Mq.Testing.Tests.BadHandlers
{
    public class HelloAndByeAsyncHandler : IAsyncMessageHandler<HelloMessage>, IAsyncMessageHandler<ByeMessage>
    {
        public Task HandleAsync(HelloMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task HandleAsync(ByeMessage message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task HandleAsync(object message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
