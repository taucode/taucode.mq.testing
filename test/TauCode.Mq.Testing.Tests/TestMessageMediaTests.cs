using NUnit.Framework;
using TauCode.Mq.Testing.Tests.Handlers;

namespace TauCode.Mq.Testing.Tests
{
    [TestFixture]
    public class TestMessageMediaTests
    {
        [SetUp]
        public void SetUp()
        {
            DemoLog.Instance.StringBuilder.Clear();
        }

        [Test]
        public void Publish_Topic_BothTopicAndTopiclessInvoked()
        {
            // Arrange
            var media = new TestMessageMedia();
            var publisher = new TestMessagePublisher(media);
            var subscriber = new TestMessageSubscriber(media, new DemoFactory());

            subscriber.Subscribe(typeof(AllHandler));
            subscriber.Subscribe(typeof(SummerHandler), "summer");
            subscriber.Subscribe(typeof(WinterHandler), "winter");

            publisher.Start();
            subscriber.Start();

            // Act
            publisher.Publish(new DemoMessage("No topic"));
            publisher.Publish(new DemoMessage("Topic is Summer"), "summer");
            publisher.Publish(new DemoMessage("Topic is Winter"), "winter");

            // Assert
            var log = DemoLog.Instance.StringBuilder.ToString();
            Assert.That(
                log, 
                Is.EqualTo(@"[TauCode.Mq.Testing.Tests.Handlers.AllHandler]: No topic
[TauCode.Mq.Testing.Tests.Handlers.AllHandler]: Topic is Summer
[TauCode.Mq.Testing.Tests.Handlers.SummerHandler]: Topic is Summer
[TauCode.Mq.Testing.Tests.Handlers.AllHandler]: Topic is Winter
[TauCode.Mq.Testing.Tests.Handlers.WinterHandler]: Topic is Winter
"));

            publisher.Dispose();
            subscriber.Dispose();
        }
    }
}
