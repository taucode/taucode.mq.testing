using System;
using TauCode.Mq.Testing.Tests.Handlers;

namespace TauCode.Mq.Testing.Tests
{
    public class DemoContext : IMessageHandlerContext
    {
        public void Dispose()
        {
            // idle
        }

        public void Begin()
        {
            // idle
        }

        public void End()
        {
            // idle
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(AllHandler))
            {
                return new AllHandler();
            }
            else if (serviceType == typeof(SummerHandler))
            {
                return new SummerHandler();
            }
            else if (serviceType == typeof(WinterHandler))
            {
                return new WinterHandler();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
