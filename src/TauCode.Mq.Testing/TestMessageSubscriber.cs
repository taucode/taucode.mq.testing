using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TauCode.Mq.Testing
{
    public class TestMessageSubscriber : MessageSubscriberBase
    {
        #region Fields

        private readonly ITestMqMedia _media;
        private readonly List<IDisposable> _handles;

        #endregion

        #region Constructor

        public TestMessageSubscriber(ITestMqMedia media, IMessageHandlerContextFactory contextFactory)
            : base(contextFactory)
        {
            _media = media ?? throw new ArgumentNullException(nameof(media));
            _handles = new List<IDisposable>();
        }

        #endregion

        #region Overridden

        protected override void InitImpl()
        {
            // idle
        }

        protected override void ShutdownImpl()
        {
            foreach (var handle in _handles)
            {
                handle.Dispose();
            }
        }

        protected override IDisposable SubscribeImpl(ISubscriptionRequest subscriptionRequest)
        {
            if (subscriptionRequest.AsyncHandler != null && subscriptionRequest.Handler == null)
            {
                // got async subscription
                if (subscriptionRequest.Topic == null)
                {
                    return _media.Subscribe(
                        subscriptionRequest.MessageType,
                        subscriptionRequest.AsyncHandler);
                }
                else
                {
                    return _media.Subscribe(
                        subscriptionRequest.MessageType,
                        subscriptionRequest.AsyncHandler,
                        subscriptionRequest.Topic);
                }
            }
            else if (subscriptionRequest.Handler != null && subscriptionRequest.AsyncHandler == null)
            {
                // got sync subscription
                if (subscriptionRequest.Topic == null)
                {
                    return _media.Subscribe(
                        subscriptionRequest.MessageType,
                        obj =>
                        {
                            subscriptionRequest.Handler(obj);
                            return Task.CompletedTask;
                        });
                }
                else
                {
                    return _media.Subscribe(
                        subscriptionRequest.MessageType,
                        obj =>
                        {
                            subscriptionRequest.Handler(obj);
                            return Task.CompletedTask;
                        },
                        subscriptionRequest.Topic);
                }
            }
            else
            {
                throw new ArgumentException(
                    $"'{nameof(subscriptionRequest)}' is invalid.",
                    nameof(subscriptionRequest));
            }
        }

        #endregion
    }
}
