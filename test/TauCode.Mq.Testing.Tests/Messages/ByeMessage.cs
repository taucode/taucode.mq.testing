using System;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing.Tests.Messages
{
    public class ByeMessage : IMessage
    {
        public ByeMessage()
        {   
        }

        public ByeMessage(string nickname)
        {
            this.Nickname = nickname;
        }

        public string Topic { get; set; }
        public string CorrelationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Nickname { get; set; }
        public int MillisecondsTimeout { get; set; } = 0;
    }
}
