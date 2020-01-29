using System;
using System.Collections.Generic;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing
{
    public class TestMessageMedia
    {
        private class Subscription
        {
            private readonly IList<Action<object>> _nullTopicHandlers;
            private readonly Dictionary<string, IList<Action<object>>> _topicHandlers;

            public Subscription(Type messageType)
            {
                this.MessageType = messageType;
                _nullTopicHandlers = new List<Action<object>>();
                _topicHandlers = new Dictionary<string, IList<Action<object>>>();
            }

            public Type MessageType { get; }

            public void AddHandler(Action<object> handler)
            {
                _nullTopicHandlers.Add(handler);
            }

            public void AddHandler(Action<object> handler, string topic)
            {
                var haveTopic = _topicHandlers.TryGetValue(topic, out var list);
                if (!haveTopic)
                {
                    list = new List<Action<object>>();
                    _topicHandlers.Add(topic, list);
                }

                list.Add(handler);
            }

            public void Handle(IMessage message)
            {
                foreach (var handler in _nullTopicHandlers)
                {
                    handler(message);
                }
            }

            public void Handle(IMessage message, string topic)
            {
                foreach (var handler in _nullTopicHandlers)
                {
                    handler(message);
                }

                var gotTopic = _topicHandlers.TryGetValue(topic, out var list);
                if (gotTopic)
                {
                    foreach (var handler in list)
                    {
                        handler(message);
                    }
                }

                // todo clean
                //throw new NotImplementedException();

                //foreach (var handler in _nullTopicHandlers)
                //{
                //    handler(message);
                //}
            }
        }

        private readonly Dictionary<Type, Subscription> _subscriptions;

        public TestMessageMedia()
        {
            _subscriptions = new Dictionary<Type, Subscription>();
        }

        public void Publish(IMessage message, string topic)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic));
            }

            var messageType = message.GetType();
            _subscriptions.TryGetValue(messageType, out var subscription);

            subscription?.Handle(message, topic);
        }

        public void Publish(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageType = message.GetType();
            _subscriptions.TryGetValue(messageType, out var subscription);

            subscription?.Handle(message);
        }

        public void Subscribe(Type messageType, Action<object> handler)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var alreadyHave = _subscriptions.TryGetValue(messageType, out var subscription);
            if (!alreadyHave)
            {
                subscription = new Subscription(messageType);
                _subscriptions.Add(messageType, subscription);
            }

            subscription.AddHandler(handler);
        }

        public void Subscribe(Type messageType, Action<object> handler, string topic)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic));
            }

            var alreadyHave = _subscriptions.TryGetValue(messageType, out var subscription);
            if (!alreadyHave)
            {
                subscription = new Subscription(messageType);
                _subscriptions.Add(messageType, subscription);
            }

            subscription.AddHandler(handler, topic);
        }
    }
}
