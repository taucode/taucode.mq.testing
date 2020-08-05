using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing.Tests.Handlers
{
    public class HandlerBase : MessageHandlerBase<DemoMessage>
    {
        public override void Handle(DemoMessage message)
        {
            var msg = $"[{this.GetType().FullName}]: {message.Text}";
            DemoLog.Instance.StringBuilder.AppendLine(msg);
        }
    }
}
