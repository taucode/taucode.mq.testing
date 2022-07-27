using System;
using System.Threading.Tasks;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing
{
    public interface ITestMqMedia : IDisposable
    {
        void Publish(Type messageType, IMessage message);

        IDisposable Subscribe(Type messageType, Func<IMessage, Task> handler);

        IDisposable Subscribe(Type messageType, Func<IMessage, Task> handler, string topic);
    }
}
