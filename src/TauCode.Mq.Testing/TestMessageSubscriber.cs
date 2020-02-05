using System;
using System.Collections.Generic;

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

        protected override void SubscribeImpl(IEnumerable<ISubscriptionRequest> requests)
        {
            foreach (var request in requests)
            {
                if (request.Topic == null)
                {
                    this.Media.Subscribe(request.MessageType, request.Handler);
                }
                else
                {
                    this.Media.Subscribe(request.MessageType, request.Handler, request.Topic);
                }
            }
        }

        protected override void UnsubscribeImpl()
        {
            // todo: refactor test media for unsubscription.
        }
    }
}
