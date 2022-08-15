namespace TauCode.Mq.Testing;

public interface ITestMqMedia : IDisposable
{
    void Publish(Type messageType, IMessage message);

    IDisposable Subscribe(Type messageType, Func<IMessage, CancellationToken, Task> handler);

    IDisposable Subscribe(Type messageType, Func<IMessage, CancellationToken, Task> handler, string topic);
}