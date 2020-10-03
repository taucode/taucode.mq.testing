using System;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing.Tests.Messages
{
    public struct StructMessage : IMessage
    {
        public string Topic { get; set; }
        public string CorrelationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Category { get; set; }
    }
}
