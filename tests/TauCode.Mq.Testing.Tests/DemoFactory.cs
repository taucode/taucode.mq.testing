namespace TauCode.Mq.Testing.Tests
{
    public class DemoFactory : IMessageHandlerContextFactory
    {
        public IMessageHandlerContext CreateContext() => new DemoContext();

        //public IMessageHandler CreateHandler(IMessageHandlerContext context, Type handlerType)
        //{
        //    if (handlerType == typeof(AllHandler))
        //    {
        //        return new AllHandler();
        //    }
        //    else if (handlerType == typeof(SummerHandler))
        //    {
        //        return new SummerHandler();
        //    }
        //    else if (handlerType == typeof(WinterHandler))
        //    {
        //        return new WinterHandler();
        //    }
        //    else
        //    {
        //        throw new NotSupportedException();
        //    }
        //}
    }
}
