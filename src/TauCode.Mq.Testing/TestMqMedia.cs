using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace TauCode.Mq.Testing
{
    public class TestMqMedia : ITestMqMedia
    {
        #region Nested

        private class MediaSubscription
        {
            private readonly Dictionary<string, Func<object, Task>> _handlers;

            internal MediaSubscription(Type messageType, string topic)
            {
                this.MessageType = messageType;
                this.Topic = topic;
                this.Tag = BuildTag(messageType, topic);
                _handlers = new Dictionary<string, Func<object, Task>>();
            }

            internal string Tag { get; }
            internal Type MessageType { get; }
            internal string Topic { get; }

            internal string AddHandler(Func<object, Task> handler)
            {
                var id = Guid.NewGuid().ToString();
                _handlers.Add(id, handler);

                return id;
            }

            internal IReadOnlyList<Func<object, Task>> GetHandlers() => _handlers.Values.ToList();

            internal bool RemoveHandler(string id)
            {
                return _handlers.Remove(id);
            }
        }

        #endregion

        #region Fields

        private readonly MessageQueue _messageQueue;
        private readonly object _lock;
        private readonly Dictionary<string, MediaSubscription> _subscriptions;

        #endregion

        #region Constructor

        public TestMqMedia()
        {
            _lock = new object();
            _subscriptions = new Dictionary<string, MediaSubscription>();
            _messageQueue = new MessageQueue(this);
            _messageQueue.Start();
        }

        #endregion

        #region Private

        private IDisposable SubscribeImpl(Type messageType, Func<object, Task> handler, string topic)
        {
            var tag = BuildTag(messageType, topic);

            lock (_lock)
            {
                var subscription = _subscriptions.GetValueOrDefault(tag);
                if (subscription == null)
                {
                    subscription = new MediaSubscription(messageType, topic);
                    _subscriptions.Add(subscription.Tag, subscription);
                }

                var id = subscription.AddHandler(handler);
                return new SubscriptionHandle(this, tag, id);
            }
        }

        #endregion

        #region Internal

        internal async Task DispatchMessagePackage(MessagePackage messagePackage)
        {
            var allTag = BuildTag(messagePackage.MessageType, null);
            var topicTag = "<non_existing_tag>";

            if (messagePackage.Topic != null)
            {
                topicTag = BuildTag(messagePackage.MessageType, messagePackage.Topic);
            }

            IEnumerable<Func<object, Task>> allHandlers;
            IEnumerable<Func<object, Task>> topicHandlers;

            lock (_lock)
            {
                var allSubscription = _subscriptions.GetValueOrDefault(allTag);
                var topicSubscription = _subscriptions.GetValueOrDefault(topicTag);

                allHandlers = allSubscription?.GetHandlers() ?? new List<Func<object, Task>>();
                topicHandlers = topicSubscription?.GetHandlers() ?? new List<Func<object, Task>>();
            }

            foreach (var handler in allHandlers)
            {
                try
                {
                    var message = JsonConvert.DeserializeObject(messagePackage.MessageJson, messagePackage.MessageType);
                    await handler(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Handler threw an exception.");
                }
            }

            foreach (var handler in topicHandlers)
            {
                try
                {
                    var message = JsonConvert.DeserializeObject(messagePackage.MessageJson, messagePackage.MessageType);
                    await handler(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Handler threw an exception.");
                }
            }
        }

        internal void Unsubscribe(string tag, string id)
        {
            lock (_lock)
            {
                var subscription = _subscriptions.GetValueOrDefault(tag);
                subscription?.RemoveHandler(id);
            }
        }

        internal static string BuildTag(Type messageType, string topic)
        {
            topic ??= string.Empty;

            return $"[{messageType.FullName}:{topic}]";
        }

        #endregion

        #region ITestMqMedia Members

        public void Publish(Type messageType, object message)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.GetType() != messageType)
            {
                throw new ArgumentException("Message type mismatch", nameof(message));
            }

            var json = JsonConvert.SerializeObject(message);
            _messageQueue.AddAssignment(new MessagePackage(messageType, json, null));
        }

        public void Publish(Type messageType, object message, string topic)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.GetType() != messageType)
            {
                throw new ArgumentException("Message type mismatch", nameof(message));
            }

            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException($"'{nameof(topic)}' cannot be null or empty.", nameof(topic));
            }

            var json = JsonConvert.SerializeObject(message);
            _messageQueue.AddAssignment(new MessagePackage(messageType, json, topic));
        }

        public IDisposable Subscribe(Type messageType, Func<object, Task> handler)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return this.SubscribeImpl(messageType, handler, null);
        }

        public IDisposable Subscribe(Type messageType, Func<object, Task> handler, string topic)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException($"'{nameof(topic)}' cannot be null or empty.", nameof(topic));
            }

            return this.SubscribeImpl(messageType, handler, topic);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _messageQueue.Dispose();
        }

        #endregion
    }
}
