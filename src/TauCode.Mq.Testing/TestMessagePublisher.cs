using System;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing
{
    public class TestMessagePublisher : MessagePublisherBase
    {
        public TestMessagePublisher(TestMessageMedia media)
        {
            this.Media = media ?? throw new ArgumentNullException(nameof(media));
        }

        public TestMessageMedia Media { get; }

        protected override void PublishImpl(IMessage message)
        {
            this.Media.Publish(message);
        }

        protected override void PublishImpl(IMessage message, string topic)
        {
            this.Media.Publish(message, topic);
        }
    }
}
