using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using TauCode.Mq.Testing.Tests.Handlers.Bye.Async;
using TauCode.Mq.Testing.Tests.Handlers.Bye.Sync;
using TauCode.Mq.Testing.Tests.Handlers.Hello.Async;
using TauCode.Mq.Testing.Tests.Handlers.Hello.Sync;

namespace TauCode.Mq.Testing.Tests.Contexts
{
    public class GoodContext : IMessageHandlerContext
    {
        private static readonly HashSet<Type> SupportedHandlerTypes = new[]
            {
                typeof(BeBackAsyncHandler),
                typeof(ByeAsyncHandler),

                typeof(ByeHandler),

                typeof(CancelingHelloAsyncHandler),
                typeof(FaultingHelloAsyncHandler),
                typeof(FishHaterAsyncHandler),
                typeof(HelloAsyncHandler),
                typeof(WelcomeAsyncHandler),

                typeof(FishHaterHandler),
                typeof(HelloHandler),
                typeof(WelcomeHandler),
            }
            .ToHashSet();

        public void Begin()
        {
            Log.Debug("Context began.");
        }

        public object GetService(Type serviceType)
        {
            if (SupportedHandlerTypes.Contains(serviceType))
            {
                var ctor = serviceType.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    throw new NotSupportedException($"Type '{serviceType.FullName}' has no parameterless constructor.");
                }

                var service = ctor.Invoke(new object[] { });
                return service;
            }

            throw new NotSupportedException($"Service of type '{serviceType.FullName}' is not supported.");
        }

        public void End()
        {
            Log.Debug("Context ended.");
        }

        public void Dispose()
        {
            // idle
        }
    }
}
