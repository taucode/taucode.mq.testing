using System;
using System.Threading.Tasks;

namespace TauCode.Mq.Testing
{
    public interface ITestMqMedia : IDisposable
    {
        void Publish(Type messageType, object message);
        void Publish(Type messageType, object message, string topic);

        IDisposable Subscribe(Type messageType, Func<object, Task> handler);
        IDisposable Subscribe(Type messageType, Func<object, Task> handler, string topic);
    }
}
