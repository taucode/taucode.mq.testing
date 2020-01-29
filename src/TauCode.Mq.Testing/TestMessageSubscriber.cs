using System;

namespace TauCode.Mq.Testing
{
    public class TestMessageSubscriber : MessageSubscriberBase
    {
        public TestMessageSubscriber(TestMessageMedia media, IMessageHandlerContextFactory contextFactory)
            : base(contextFactory)
        {
            this.Media = media ?? throw new ArgumentNullException(nameof(media));
        }

        public TestMessageMedia Media { get; }

        protected override void StartImpl()
        {
            base.StartImpl();

            foreach (var pair in this.Bundles)
            {
                var bundle = pair.Value;
                var topic = bundle.Topic;

                if (topic == null)
                {
                    this.Media.Subscribe(bundle.MessageType, bundle.Handle);
                }
                else
                {
                    this.Media.Subscribe(bundle.MessageType, bundle.Handle, topic);
                }
            }
        }
    }
}
