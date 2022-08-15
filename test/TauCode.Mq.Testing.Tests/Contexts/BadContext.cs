using TauCode.Mq.Testing.Tests.Handlers.Bye.Async;
using TauCode.Mq.Testing.Tests.Handlers.Hello.Async;

namespace TauCode.Mq.Testing.Tests.Contexts;

public class BadContext : IMessageHandlerContext
{
    private readonly bool _throwOnBegin;
    private readonly bool _throwOnEnd;

    private readonly bool _throwOnGetService;
    private readonly bool _returnNullOnGetService;
    private readonly bool _returnsWrongService;

    private readonly bool _throwOnDispose;

    public BadContext(
        bool throwOnBegin,
        bool throwOnEnd,
        bool throwOnGetService,
        bool returnNullOnGetService,
        bool returnsWrongService,
        bool throwOnDispose)
    {
        _throwOnBegin = throwOnBegin;
        _throwOnEnd = throwOnEnd;

        _throwOnGetService = throwOnGetService;
        _returnNullOnGetService = returnNullOnGetService;
        _returnsWrongService = returnsWrongService;

        _throwOnDispose = throwOnDispose;
    }

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_throwOnBegin)
        {
            throw new NotSupportedException("Failed to begin.");
        }

        return Task.CompletedTask;
    }

    public object GetService(Type serviceType)
    {
        if (_throwOnGetService)
        {
            throw new NotSupportedException("Failed to get service.");
        }

        if (_returnNullOnGetService)
        {
            return null;
        }

        if (serviceType == typeof(HelloAsyncHandler))
        {
            if (_returnsWrongService)
            {
                return new ByeAsyncHandler();
            }

            return new HelloAsyncHandler();
        }

        throw new NotSupportedException($"Service of type '{serviceType.FullName}' not supported.");
    }

    public Task EndAsync(CancellationToken cancellationToken = default)
    {
        if (_throwOnEnd)
        {
            throw new NotSupportedException("Failed to end.");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_throwOnDispose)
        {
            throw new NotSupportedException("Failed to dispose.");
        }
    }
}