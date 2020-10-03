using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TauCode.Mq.Testing.Tests.Messages;
using TauCode.Working;
using TauCode.Working.Exceptions;

namespace TauCode.Mq.Testing.Tests
{
    [TestFixture]
    public class TestMessagePublisherTests
    {
        private ITestMqMedia _media;

        [SetUp]
        public void SetUp()
        {
            _media = new TestMqMedia();
        }

        [TearDown]
        public void TearDown()
        {
            _media.Dispose();
        }

        #region ctor

        [Test]
        public void Constructor_NoArguments_RunsOk()
        {
            // Arrange

            // Act
            using IMessagePublisher messagePublisher = new TestMessagePublisher(_media);

            // Assert
            Assert.Pass();
        }

        #endregion

        #region Publish(IMessage)

        [Test]
        public void PublishIMessage_ValidStateAndArgument_PublishesAndProperSubscriberHandles()
        {
            // Arrange

            // Act

            // Assert
            Assert.Pass($"See '{nameof(PublishIMessageString_ValidStateAndArguments_PublishesAndProperSubscriberHandles)}', both methods are UT'ed there.");
        }

        [Test]
        public void PublishIMessage_ArgumentIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => publisher.Publish(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void PublishIMessage_ArgumentIsNotClass_ThrowsArgumentException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => publisher.Publish(new StructMessage()));

            // Assert
            Assert.That(ex,
                Has.Message.StartWith(
                    $"Cannot publish instance of '{typeof(StructMessage).FullName}'. Message type must be a class."));
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void PublishIMessage_ArgumentPropertyThrows_ThrowsJsonSerializationException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            var ex = Assert.Throws<JsonSerializationException>(() =>
            {
                publisher.Publish(new ThrowPropertyMessage
                {
                    BadProperty = "bad",
                });
            });

            // Assert
            Assert.That(ex.InnerException, Is.TypeOf<NotSupportedException>());
            Assert.That(ex.InnerException, Has.Message.EqualTo("Property is bad!"));
        }

        [Test]
        public void PublishIMessage_NotStarted_ThrowsMqException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);

            // Act
            var ex = Assert.Throws<InappropriateWorkerStateException>(() => publisher.Publish(new HelloMessage()));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Inappropriate worker state (Stopped)."));
        }

        [Test]
        public void PublishIMessage_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new HelloMessage()));

            // Assert
            Assert.That(ex.ObjectName, Is.EqualTo("my-publisher"));
        }

        #endregion

        #region Publish(IMessage, string)

        [Test]
        public async Task PublishIMessageString_ValidStateAndArguments_PublishesAndProperSubscriberHandles()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);

            publisher.Start();

            string name = null;
            string nameWithTopic = null;

            string name1;
            string nameWithTopic1;

            string name2;
            string nameWithTopic2;

            using var sub1 = _media.Subscribe<HelloMessage>(message => name = message.Name);

            using var sub2 = _media.Subscribe<HelloMessage>(
                message => nameWithTopic = message.Name,
                "some-topic");

            // Act
            publisher.Publish(new HelloMessage
                {
                    Name = "mia",
                },
                "some-topic");

            await Task.Delay(100);

            name1 = name;
            nameWithTopic1 = nameWithTopic;

            publisher.Publish(new HelloMessage
            {
                Name = "deserea",
            });

            await Task.Delay(100);

            name2 = name;
            nameWithTopic2 = nameWithTopic;

            // Assert
            Assert.That(name1, Is.EqualTo("mia"));
            Assert.That(nameWithTopic1, Is.EqualTo("mia"));

            Assert.That(name2, Is.EqualTo("deserea"));
            Assert.That(nameWithTopic2, Is.EqualTo("mia"));
        }

        [Test]
        public void PublishIMessageString_MessageIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => publisher.Publish(null, "some-topic"));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void PublishIMessageString_MessageIsNotClass_ThrowsArgumentException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => publisher.Publish(new StructMessage()));

            // Assert
            Assert.That(ex,
                Has.Message.StartWith(
                    $"Cannot publish instance of '{typeof(StructMessage).FullName}'. Message type must be a class."));
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void PublishIMessageString_TopicIsNullOrEmpty_ThrowsTodo(string badTopic)
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => publisher.Publish(new HelloMessage(), badTopic));

            // Assert
            Assert.That(ex,
                Has.Message.StartWith(
                    "'topic' cannot be null or empty. If you need to publish a topicless message, use the 'Publish(IMessage message)' overload."));
            Assert.That(ex.ParamName, Is.EqualTo("topic"));
        }

        [Test]
        public void PublishIMessageString_NotStarted_ThrowsMqException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);

            // Act
            var ex = Assert.Throws<InappropriateWorkerStateException>(() => publisher.Publish(new HelloMessage(), "some-topic"));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Inappropriate worker state (Stopped)."));
        }

        [Test]
        public void PublishIMessageString_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher",
            };

            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new HelloMessage(), "my-topic"));

            // Assert
            Assert.That(ex.ObjectName, Is.EqualTo("my-publisher"));
        }

        #endregion

        #region Name

        [Test]
        public void Name_NotDisposed_IsChangedAndCanBeRead()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "pub_created",
            };

            // Act
            var nameCreated = publisher.Name;

            publisher.Start();
            publisher.Name = "pub_started";

            var nameStarted = publisher.Name;

            publisher.Stop();
            publisher.Name = "pub_stopped";

            var nameStopped = publisher.Name;

            // Assert
            Assert.That(nameCreated, Is.EqualTo("pub_created"));
            Assert.That(nameStarted, Is.EqualTo("pub_started"));
            Assert.That(nameStopped, Is.EqualTo("pub_stopped"));
        }

        [Test]
        public void Name_Disposed_CanOnlyBeRead()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "name1",
            };

            // Act
            publisher.Dispose();

            var name = publisher.Name;
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Name = "name2");

            // Assert
            Assert.That(name, Is.EqualTo("name1"));
            Assert.That(publisher.Name, Is.EqualTo("name1"));
            Assert.That(ex.ObjectName, Is.EqualTo("name1"));
        }

        #endregion

        #region State

        [Test]
        public void State_JustCreated_EqualsToStopped()
        {
            // Arrange

            // Act
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        [Test]
        public void State_Started_EqualsToRunning()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            // Act
            publisher.Start();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Running));
        }

        [Test]
        public void State_Stopped_EqualsToStopped()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();

            // Act
            publisher.Stop();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        [Test]
        public void State_DisposedJustAfterCreation_EqualsToStopped()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        [Test]
        public void State_DisposedAfterStarted_EqualsToStopped()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        [Test]
        public void State_DisposedAfterStopped_EqualsToStopped()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();
            publisher.Stop();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        [Test]
        public void State_DisposedAfterDisposed_EqualsToStopped()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();
            publisher.Stop();
            publisher.Dispose();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        #endregion

        #region IsDisposed

        [Test]
        public void IsDisposed_JustCreated_EqualsToFalse()
        {
            // Arrange

            // Act
            using var publisher = new TestMessagePublisher(_media);

            // Assert
            Assert.That(publisher.IsDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_Started_EqualsToFalse()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);

            // Act
            publisher.Start();

            // Assert
            Assert.That(publisher.IsDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_Stopped_EqualsToFalse()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            publisher.Stop();

            // Assert
            Assert.That(publisher.IsDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_DisposedJustAfterCreation_EqualsToTrue()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.IsDisposed, Is.True);
        }

        [Test]
        public void IsDisposed_DisposedAfterStarted_EqualsToTrue()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Start();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.IsDisposed, Is.True);
        }

        [Test]
        public void IsDisposed_DisposedAfterStopped_EqualsToTrue()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);

            publisher.Start();
            publisher.Stop();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.IsDisposed, Is.True);
        }

        [Test]
        public void IsDisposed_DisposedAfterDisposed_EqualsToTrue()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media);
            publisher.Dispose();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.IsDisposed, Is.True);
        }

        #endregion

        #region Start()

        [Test]
        public void Start_JustCreated_Starts()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            // Act
            publisher.Start();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Start_Started_ThrowsInappropriateWorkerStateException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();

            // Act
            var ex = Assert.Throws<InappropriateWorkerStateException>(() => publisher.Start());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Inappropriate worker state (Running)."));
        }

        [Test]
        public void Start_Stopped_Starts()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {   
                Name = "my-publisher"
            };

            publisher.Start();
            publisher.Stop();

            // Act
            publisher.Start();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Start_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };
            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Start());

            // Assert
            Assert.That(ex.ObjectName, Is.EqualTo("my-publisher"));
        }

        #endregion

        #region Stop()

        [Test]
        public void Stop_JustCreated_ThrowsInappropriateWorkerStateException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            // Act
            var ex = Assert.Throws<InappropriateWorkerStateException>(() => publisher.Stop());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Inappropriate worker state (Stopped)."));
        }

        [Test]
        public void Stop_Started_Stops()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();

            // Act
            publisher.Stop();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Stop_Stopped_ThrowsInappropriateWorkerStateException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();
            publisher.Stop();

            // Act
            var ex = Assert.Throws<InappropriateWorkerStateException>(() => publisher.Stop());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Inappropriate worker state (Stopped)."));
        }

        [Test]
        public void Stop_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };
            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Stop());

            // Assert
            Assert.That(ex.ObjectName, Is.EqualTo("my-publisher"));
        }

        #endregion

        #region Dispose()

        [Test]
        public void Dispose_JustCreated_Disposes()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            // Act
            publisher.Dispose();

            // Assert
            Assert.Pass("Test passed.");
        }

        [Test]
        public void Dispose_Started_Disposes()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();

            // Act
            publisher.Dispose();

            // Assert
            Assert.Pass("Test passed.");
        }

        [Test]
        public void Dispose_Stopped_Disposes()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Start();
            publisher.Stop();

            // Act
            publisher.Dispose();

            // Assert
            Assert.Pass("Test passed.");
        }

        [Test]
        public void Disposes_Disposed_DoesNothing()
        {
            // Arrange
            using var publisher = new TestMessagePublisher(_media)
            {
                Name = "my-publisher"
            };

            publisher.Dispose();

            // Act
            publisher.Dispose();

            // Assert
            Assert.Pass("Test passed.");
        }

        #endregion
    }
}