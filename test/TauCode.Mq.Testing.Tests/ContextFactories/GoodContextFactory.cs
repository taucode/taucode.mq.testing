using TauCode.Mq.Testing.Tests.Contexts;

namespace TauCode.Mq.Testing.Tests.ContextFactories
{
    public class GoodContextFactory : IMessageHandlerContextFactory
    {
        public IMessageHandlerContext CreateContext()
        {
            return new GoodContext();
        }
    }
}
