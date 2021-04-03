using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TauCode.Infrastructure.Logging;
using TauCode.Mq.Testing.Tests.Messages;
using TauCode.Working;

namespace TauCode.Mq.Testing.Tests
{
    [TestFixture]
    public class TestMessagePublisherTests
    {
        private StringLogger _logger;
        private ITestMqMedia _media;

        [SetUp]
        public void SetUp()
        {
            _logger = new StringLogger();
            _media = new TestMqMedia(_logger);

            var collection = new LoggerProviderCollection();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Providers(collection)
                .MinimumLevel
                .Debug()
                .CreateLogger();

            var providerMock = new Mock<ILoggerProvider>();
            providerMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_logger);

            collection.AddProvider(providerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _media.Dispose();
        }

        private IMessagePublisher CreateMessagePublisher(string name = null)
        {
            var messagePublisher = new TestMessagePublisher(_media)
            {
                Name = name,
            };

            return messagePublisher;
        }

        #region ctor

        [Test]
        public void Constructor_ValidMedia_RunsOk()
        {
            // Arrange

            // Act
            using IMessagePublisher messagePublisher = new TestMessagePublisher(_media);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Constructor_MediaIsNull_ThrowsException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new TestMessagePublisher(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("media"));
        }

        #endregion

        #region Publish(IMessage)

        [Test]
        public async Task Publish_ValidStateAndArguments_PublishesAndProperSubscriberHandles()
        {
            using var publisher = this.CreateMessagePublisher();

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
            publisher.Publish(
                new HelloMessage
                {
                    Name = "mia",
                    Topic = "some-topic",
                });

            await Task.Delay(150);

            name1 = name;
            nameWithTopic1 = nameWithTopic;

            publisher.Publish(new HelloMessage
            {
                Name = "deserea",
            });

            await Task.Delay(150);

            name2 = name;
            nameWithTopic2 = nameWithTopic;

            // Assert
            Assert.That(name1, Is.EqualTo("mia"));
            Assert.That(nameWithTopic1, Is.EqualTo("mia"));

            Assert.That(name2, Is.EqualTo("deserea"));
            Assert.That(nameWithTopic2, Is.EqualTo("mia"));
        }

        [Test]
        public void Publish_ArgumentIsNull_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => publisher.Publish(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void Publish_ArgumentIsNotClassNoTopic_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => publisher.Publish(new StructMessage()));

            // Assert
            Assert.That(
                ex,
                Has.Message.StartWith(
                    $"Cannot publish instance of '{typeof(StructMessage).FullName}'. Message type must be a class."));
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void Publish_ArgumentIsNotClassWithTopic_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

            publisher.Start();

            // Act
            var ex = Assert.Throws<ArgumentException>(() => publisher.Publish(new StructMessage
            {
                Topic = "some-topic",
            }));

            // Assert
            Assert.That(ex,
                Has.Message.StartWith(
                    $"Cannot publish instance of '{typeof(StructMessage).FullName}'. Message type must be a class."));
            Assert.That(ex.ParamName, Is.EqualTo("message"));
        }

        [Test]
        public void Publish_ArgumentPropertyThrows_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

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
        public void Publish_NoTopicNotStarted_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => publisher.Publish(new HelloMessage()));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Publish'. Worker state is 'Stopped'."));
        }

        [Test]
        public void Publish_WithTopicNotStarted_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => publisher.Publish(
                new HelloMessage
                {
                    Topic = "some-topic",
                }));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Publish'. Worker state is 'Stopped'."));
        }

        [Test]
        public void Publish_NoTopicDisposed_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new HelloMessage()));

            // Assert
            Assert.That(ex.ObjectName, Is.EqualTo("my-publisher"));
        }

        [Test]
        public void Publish_WithTopicDisposed_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Publish(new HelloMessage
            {
                Topic = "my-topic",
            }));

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
                Name = "name1"
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
            using var publisher = this.CreateMessagePublisher();

            // Act
            publisher.Start();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Running));
        }

        [Test]
        public void State_Stopped_EqualsToStopped()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.State, Is.EqualTo(WorkerState.Stopped));
        }

        [Test]
        public void State_DisposedAfterStarted_EqualsToStopped()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

            // Act
            publisher.Start();

            // Assert
            Assert.That(publisher.IsDisposed, Is.False);
        }

        [Test]
        public void IsDisposed_Stopped_EqualsToFalse()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

            // Act
            publisher.Dispose();

            // Assert
            Assert.That(publisher.IsDisposed, Is.True);
        }

        [Test]
        public void IsDisposed_DisposedAfterStarted_EqualsToTrue()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

            // Act
            publisher.Start();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Start_Started_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

            publisher.Start();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => publisher.Start());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'my-publisher'."));
        }

        [Test]
        public void Start_Stopped_Starts()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();


            publisher.Start();
            publisher.Stop();

            // Act
            publisher.Start();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Start_Disposed_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

            publisher.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => publisher.Start());

            // Assert
            Assert.That(ex.ObjectName, Is.EqualTo("my-publisher"));
        }

        #endregion

        #region Stop()

        [Test]
        public void Stop_JustCreated_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => publisher.Stop());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'my-publisher'."));
        }

        [Test]
        public void Stop_Started_Stops()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

            publisher.Start();

            // Act
            publisher.Stop();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Stop_Stopped_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

            publisher.Start();
            publisher.Stop();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => publisher.Stop());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'my-publisher'."));
        }

        [Test]
        public void Stop_Disposed_ThrowsException()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher("my-publisher");

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
            using var publisher = this.CreateMessagePublisher();

            // Act
            publisher.Dispose();

            // Assert
            Assert.Pass("Test passed.");
        }

        [Test]
        public void Dispose_Started_Disposes()
        {
            // Arrange
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

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
            using var publisher = this.CreateMessagePublisher();

            publisher.Dispose();

            // Act
            publisher.Dispose();

            // Assert
            Assert.Pass("Test passed.");
        }

        #endregion
    }
}