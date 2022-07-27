using System;
using TauCode.Mq.Abstractions;

namespace TauCode.Mq.Testing
{
    public class TestMessagePublisher : MessagePublisherBase
    {
        #region Fields

        private readonly ITestMqMedia _media;

        #endregion

        #region Constructor

        public TestMessagePublisher(ITestMqMedia media)
        {
            _media = media ?? throw new ArgumentNullException(nameof(media));
        }

        #endregion

        #region Overridden

        protected override void InitImpl()
        {
            // idle
        }

        protected override void ShutdownImpl()
        {
            // idle
        }

        protected override void PublishImpl(IMessage message)
        {
            _media.Publish(message.GetType(), message);
        }

        #endregion
    }
}
