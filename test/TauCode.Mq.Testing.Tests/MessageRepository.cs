using System.Collections.Generic;
using System.Linq;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing.Tests
{
    public class MessageRepository
    {
        public static MessageRepository Instance = new MessageRepository();

        private readonly List<IMessage> _messages;
        private readonly object _lock;

        private MessageRepository()
        {
            _messages = new List<IMessage>();
            _lock = new object();
        }

        public void Clear()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
        }

        public void Add(IMessage message)
        {
            lock (_lock)
            {
                _messages.Add(message);
            }
        }

        public IReadOnlyList<IMessage> GetMessages()
        {
            lock (_lock)
            {
                return _messages.ToList();
            }
        }
    }
}
