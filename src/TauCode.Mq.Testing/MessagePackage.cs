using System;

namespace TauCode.Mq.Testing
{
    internal class MessagePackage
    {
        internal MessagePackage(
            Type messageType,
            string messageJson,
            string topic)
        {
            this.MessageType = messageType;
            this.MessageJson = messageJson;
            this.Topic = topic;
        }

        internal Type MessageType { get; }
        internal string MessageJson { get; }
        internal string Topic { get; }
    }
}
