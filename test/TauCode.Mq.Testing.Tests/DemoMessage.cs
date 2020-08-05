using System;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing.Tests
{
    public class DemoMessage : IMessage
    {
        public DemoMessage(string text)
        {
            this.CorrelationId = Guid.NewGuid().ToString();
            this.CreatedAt = DateTime.UtcNow;
            this.Text = text;
        }

        public string CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
    }
}
