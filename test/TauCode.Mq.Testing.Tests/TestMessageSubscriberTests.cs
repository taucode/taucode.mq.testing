﻿using NUnit.Framework;
using Serilog;
using System.Text;
using TauCode.IO;
using TauCode.Mq.Exceptions;
using TauCode.Mq.Testing.Tests.BadHandlers;
using TauCode.Mq.Testing.Tests.ContextFactories;
using TauCode.Mq.Testing.Tests.Contexts;
using TauCode.Mq.Testing.Tests.Handlers.Bye.Async;
using TauCode.Mq.Testing.Tests.Handlers.Hello.Async;
using TauCode.Mq.Testing.Tests.Messages;
using TauCode.Working;
using ILogger = Serilog.ILogger;

// todo: check that topic, correlationId are preserved.
namespace TauCode.Mq.Testing.Tests;

[TestFixture]
public class TestMessageSubscriberTests
{
    private ILogger _logger;
    private StringWriterWithEncoding _writer = null!;

    private ITestMqMedia _media;

    [SetUp]
    public void SetUp()
    {
        MessageRepository.Instance.Clear();

        _writer = new StringWriterWithEncoding(Encoding.UTF8);
        _logger = new LoggerConfiguration()
            .WriteTo.TextWriter(
                _writer,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}]{ObjectTag} {Message}{NewLine}{Exception}"
            )
            .MinimumLevel.Verbose()
            .CreateLogger();
        Log.Logger = _logger;

        _media = new TestMqMedia(_logger);

        DecayingMessage.IsPropertyDecayed = false;
        DecayingMessage.IsCtorDecayed = false;
    }

    [TearDown]
    public void TearDown()
    {
        _media.Dispose();
    }

    private string CurrentLog => _writer.ToString();

    private IMessageSubscriber CreateMessageSubscriber(
        IMessageHandlerContextFactory factory,
        string name = null)
    {
        var subscriber = new TestMessageSubscriber(factory, _media, _logger)
        {
            Name = name,
            //Logger = _logger
        };

        return subscriber;
    }

    private IMessageSubscriber CreateMessageSubscriber<TFactory>(
        string name = null) where TFactory : IMessageHandlerContextFactory, new()
    {
        var factory = new TFactory();
        var subscriber = new TestMessageSubscriber(factory, _media, _logger)
        {
            Name = name,
            //Logger = _logger
        };
        return subscriber;
    }

    #region ctor

    [Test]
    public void Constructor_ValidArguments_RunsOk()
    {
        // Arrange
        var factory = new GoodContextFactory();

        // Act
        using var subscriber = new TestMessageSubscriber(factory, _media, _logger);

        // Assert
        Assert.That(subscriber.ContextFactory, Is.SameAs(factory));
    }

    [Test]
    public void ConstructorTwoArguments_FactoryArgumentIsNull_ThrowsException()
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => new TestMessageSubscriber(
            null,
            _media,
            _logger));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("contextFactory"));
    }

    #endregion

    #region ContextFactory

    [Test]
    public async Task ContextFactory_ThrowsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            true,
            false,
            false,
            false,
            false,
            false,
            false,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(500);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain($"Failed to create context."));
    }

    [Test]
    public async Task ContextFactory_ReturnsNullOnCreateContext_LogsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            false,
            true,
            false,
            false,
            false,
            false,
            false,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(200);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain($"Method 'CreateContext' of factory '{typeof(BadContextFactory).FullName}' returned 'null'."));
    }

    // todo: message handler failed once, next time goes well.

    [Test]
    public async Task ContextFactory_ContextBeginThrows_LogsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            false,
            false,
            true,
            false,
            false,
            false,
            false,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        //using var bus = RabbitHutch.CreateBus("host=localhost");

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(500);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Failed to begin."));
    }

    [Test]
    public async Task ContextFactory_ContextEndThrows_LogsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            false,
            false,
            false,
            true,
            false,
            false,
            false,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(500);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Failed to end."));
    }

    [Test]
    public async Task ContextFactory_ContextGetServiceThrows_LogsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            false,
            false,
            false,
            false,
            true,
            false,
            false,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(200);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Failed to get service."));
    }

    [Test]
    public async Task ContextFactory_ContextGetServiceReturnsNull_LogsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            false,
            false,
            false,
            false,
            false,
            true,
            false,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(500);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain($"Method 'GetService' of context '{typeof(BadContext).FullName}' returned 'null'."));
    }

    [Test]
    public async Task ContextFactory_ContextGetServiceReturnsBadResult_LogsException()
    {
        // Arrange
        IMessageHandlerContextFactory factory = new BadContextFactory(
            false,
            false,
            false,
            false,
            false,
            false,
            true,
            false);

        using var subscriber = this.CreateMessageSubscriber(factory);

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(
            new HelloMessage
            {
                Name = "Geki",
            });

        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain($"Method 'GetService' of context '{typeof(BadContext).FullName}' returned wrong service of type '{typeof(ByeAsyncHandler).FullName}'."));
    }

    #endregion

    #region Subscribe(Type)

    //[Test]
    //public async Task SubscribeType_SingleSyncHandler_HandlesMessagesWithAndWithoutTopic()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler));
    //    subscriber.Subscribe(typeof(WelcomeHandler), "topic1");

    //    subscriber.Start();

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    publisher.Publish(new HelloMessage("Lesia")
    //    {
    //        Topic = "topic1",
    //    });
    //    publisher.Publish(new HelloMessage("Olia"));

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'topic1'), Lesia!"));
    //    Assert.That(log, Does.Contain("Welcome sync (topic: 'topic1'), Lesia!"));

    //    Assert.That(log, Does.Contain("Hello sync (no topic), Olia!"));
    //    Assert.That(log, Does.Not.Contain("Welcome sync (no topic), Olia!"));
    //}

    // todo clean
    //[Test]
    //public async Task SubscribeType_MultipleSyncHandlers_HandleMessagesWithAndWithoutTopic()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler));
    //    subscriber.Subscribe(typeof(WelcomeHandler));

    //    subscriber.Subscribe(typeof(FishHaterHandler), "topic1");

    //    subscriber.Start();

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    publisher.Publish(new HelloMessage("Lesia")
    //    {
    //        Topic = "topic1",
    //    });
    //    publisher.Publish(new HelloMessage("Olia"));

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'topic1'), Lesia!"));
    //    Assert.That(log, Does.Contain("Welcome sync (topic: 'topic1'), Lesia!"));
    //    Assert.That(log, Does.Contain("Not fish - then hi sync (topic: 'topic1'), Lesia!"));

    //    Assert.That(log, Does.Contain("Hello sync (no topic), Olia!"));
    //    Assert.That(log, Does.Contain("Welcome sync (no topic), Olia!"));

    //    Assert.That(log, Does.Not.Contain("Not fish - then hi sync (no topic), Olia!"));
    //}

    // todo clean
    //[Test]
    //public async Task SubscribeType_SingleAsyncHandler_HandlesMessagesWithAndWithoutTopic()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloAsyncHandler));
    //    subscriber.Subscribe(typeof(WelcomeHandler), "topic1");

    //    subscriber.Start();

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    publisher.Publish(new HelloMessage("Lesia")
    //    {
    //        Topic = "topic1",
    //    });
    //    publisher.Publish(new HelloMessage("Olia"));

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello async (topic: 'topic1'), Lesia!"));
    //    Assert.That(log, Does.Contain("Welcome sync (topic: 'topic1'), Lesia!"));

    //    Assert.That(log, Does.Contain("Hello async (no topic), Olia!"));
    //    Assert.That(log, Does.Not.Contain("Welcome sync (no topic), Olia!"));
    //}

    [Test]
    public async Task SubscribeType_MultipleAsyncHandlers_HandleMessagesWithAndWithoutTopic()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Subscribe(typeof(WelcomeAsyncHandler));

        subscriber.Subscribe(typeof(HelloAsyncHandler), "topic1");

        subscriber.Start();

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        // Act
        publisher.Publish(new HelloMessage("Lesia")
        {
            Topic = "topic1"
        });
        publisher.Publish(new HelloMessage("Olia"));

        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Hello async (topic: 'topic1'), Lesia!"));
        Assert.That(log, Does.Contain("Welcome async (topic: 'topic1'), Lesia!"));

        Assert.That(log, Does.Contain("Hello async (no topic), Olia!"));
        Assert.That(log, Does.Contain("Welcome async (no topic), Olia!"));
    }

    [Test]
    public void SubscribeType_TypeIsNull_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => subscriber.Subscribe(null));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    }

    [Test]
    public void SubscribeType_TypeIsAbstract_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(AbstractHandler)));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' cannot be abstract."));
    }

    [Test]
    public void SubscribeType_TypeIsNotClass_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(StructHandler)));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must represent a class."));
    }

    //[Test]
    //[TestCase(typeof(NonGenericHandler))]
    //[TestCase(typeof(NotImplementingHandlerInterface))]
    //public void SubscribeType_TypeIsNotGenericSyncOrAsyncHandler_ThrowsException(Type badHandlerType)
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(badHandlerType));

    //    // Assert
    //    Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    //    Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must implement either 'IMessageHandler<TMessage>' or 'IAsyncMessageHandler<TMessage>' in a one-time manner."));
    //}

    //[Test]
    //public void SubscribeType_TypeIsSyncAfterAsync_ThrowsException()
    //{
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloAsyncHandler));
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloHandler))); // todo: if previous 'HelloAsyncHandler' was subscribed to some topic, exception won't be thrown.

    //    // Assert
    //    Assert.That(ex, Has.Message.EqualTo($"Cannot subscribe synchronous handler '{typeof(HelloHandler).FullName}' to message '{typeof(HelloMessage)}' (no topic) because there are asynchronous handlers existing for that subscription."));
    //}

    //[Test]
    //public void SubscribeType_TypeIsAsyncAfterSync_ThrowsException()
    //{
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloHandler));
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloAsyncHandler))); // todo: if previous 'HelloHandler' was subscribed to some topic, exception won't be thrown.

    //    // Assert
    //    Assert.That(ex, Has.Message.EqualTo($"Cannot subscribe asynchronous handler '{typeof(HelloAsyncHandler).FullName}' to message '{typeof(HelloMessage)}' (no topic) because there are synchronous handlers existing for that subscription."));
    //}

    [Test]
    public void SubscribeType_TypeImplementsIMessageHandlerTMessageMoreThanOnce_ThrowsException()
    {
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(HelloAndByeHandler)));

        // Assert
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must implement 'TauCode.Mq.IMessageHandler<TMessage>' once."));
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    }

    [Test]
    public void SubscribeType_TypeImplementsIAsyncMessageHandlerTMessageMoreThanOnce_ThrowsException()
    {
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(HelloAndByeAsyncHandler)));

        // Assert
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must implement 'TauCode.Mq.IMessageHandler<TMessage>' once."));
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    }

    //[Test]
    //public void SubscribeType_TypeImplementsBothSyncAndAsync_ThrowsException()
    //{
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(BothSyncAndAsyncHandler)));

    //    // Assert
    //    Assert.That(ex, Has.Message.StartWith($"'messageHandlerType' must implement either 'IMessageHandler<TMessage>' or 'IAsyncMessageHandler<TMessage>' in a one-time manner."));
    //    Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    //}

    // todo clean
    //[Test]
    //public void SubscribeType_SyncTypeAlreadySubscribed_ThrowsException()
    //{
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler));

    //    // Act
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloHandler))); // todo: there would be no error if previous subscription was with some topic.

    //    // Assert
    //    Assert.That(ex, Has.Message.EqualTo($"Handler type '{typeof(HelloHandler).FullName}' already registered for message type '{typeof(HelloMessage).FullName}' (no topic)."));
    //}

    [Test]
    public void SubscribeType_AsyncTypeAlreadySubscribed_ThrowsException()
    {
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        // Act
        var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloAsyncHandler))); // todo: there would be no error if previous subscription was with some topic.

        // Assert
        Assert.That(ex, Has.Message.EqualTo($"Handler type '{typeof(HelloAsyncHandler).FullName}' already registered for message type '{typeof(HelloMessage).FullName}' (no topic)."));
    }

    [Test]
    [TestCase(typeof(AbstractMessageHandler))]
    [TestCase(typeof(AbstractMessageAsyncHandler))]
    public void SubscribeType_TMessageIsAbstract_ThrowsException(Type badHandlerType)
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(badHandlerType));

        // Assert
        Assert.That(ex, Has.Message.StartWith($"Cannot handle abstract message type '{typeof(AbstractMessage).FullName}'."));
        Assert.That(ex.ParamName, Is.EqualTo("messageType"));
    }

    // todo clean
    //[Test]
    //[TestCase(typeof(StructMessageHandler))]
    //[TestCase(typeof(StructMessageAsyncHandler))]
    //public void SubscribeType_TMessageIsNotClass_ThrowsException(Type badHandlerType)
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(badHandlerType));

    //    // Assert
    //    Assert.That(ex, Has.Message.StartWith($"Cannot handle non-class message type '{typeof(StructMessage).FullName}'."));
    //    Assert.That(ex.ParamName, Is.EqualTo("messageType"));
    //}

    [Test]
    public async Task SubscribeType_TMessageCtorThrows_LogsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        var message = new DecayingMessage
        {
            DecayedProperty = "fresh",
        };

        DecayingMessage.IsCtorDecayed = true;

        subscriber.Subscribe(typeof(DecayingMessageHandler));
        subscriber.Start();

        // Act
        _media.Publish(message);
        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Alas Ctor Decayed!"));
    }

    [Test]
    public async Task SubscribeType_TMessagePropertyThrows_LogsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        var message = new DecayingMessage
        {
            DecayedProperty = "fresh",
        };

        DecayingMessage.IsPropertyDecayed = true;

        subscriber.Subscribe(typeof(DecayingMessageHandler));
        subscriber.Start();

        // Act
        _media.Publish(message);
        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Alas Property Decayed!"));
    }

    // todo clean
    //[Test]
    //public async Task SubscribeType_SyncHandlerHandleThrows_LogsException()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    var message = new HelloMessage
    //    {
    //        Name = "Big Fish",
    //    };

    //    subscriber.Subscribe(typeof(HelloHandler)); // #0, will handle
    //    subscriber.Subscribe(typeof(FishHaterHandler)); // #1, will throw
    //    subscriber.Subscribe(typeof(WelcomeHandler)); // #2, will handle

    //    subscriber.Start();

    //    // Act
    //    _media.Publish(message);
    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;
    //    Assert.That(log, Does.Contain("Hello sync (no topic), Big Fish!"));
    //    Assert.That(log, Does.Contain("I hate you sync (no topic), 'Big Fish'! Exception thrown!"));
    //    Assert.That(log, Does.Contain("Welcome sync (no topic), Big Fish!"));
    //}

    // todo: context.end() should not be called if handler thrown, faulted or canceled.

    // todo: complex ut which covers all cases: sync, async, no topic, topic, another topic, dups, etc.

    [Test]
    public async Task SubscribeType_AsyncHandlerHandleAsyncThrows_LogsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        var message = new HelloMessage
        {
            Name = "Big Fish",
        };

        subscriber.Subscribe(typeof(HelloAsyncHandler)); // #0, will handle
        subscriber.Subscribe(typeof(FishHaterAsyncHandler)); // #1, will throw
        subscriber.Subscribe(typeof(WelcomeAsyncHandler)); // #2, will handle

        subscriber.Start();

        // Act
        _media.Publish(message);
        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Hello async (no topic), Big Fish!"));
        Assert.That(log, Does.Contain("I hate you async (no topic), 'Big Fish'! Exception thrown!"));
        Assert.That(log, Does.Contain("Welcome async (no topic), Big Fish!"));
    }

    [Test]
    public async Task SubscribeType_AsyncHandlerCanceledOrFaulted_RestDoRun()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler)); // #0 will say 'hello'
        subscriber.Subscribe(typeof(CancelingHelloAsyncHandler)); // #1 will cancel with message
        subscriber.Subscribe(typeof(FaultingHelloAsyncHandler)); // #2 will fault with message
        subscriber.Subscribe(typeof(WelcomeAsyncHandler)); // #3 will say 'welcome', regardless of #1 canceled and #2 faulted.

        subscriber.Start();

        // Act
        _media.Publish(new HelloMessage("Ira"));

        await Task.Delay(200);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Hello async (no topic), Ira!")); // #0
        Assert.That(log, Does.Contain("Sorry, I am cancelling async (no topic), Ira...")); // #1
        Assert.That(log, Does.Contain("Sorry, I am faulting async (no topic), Ira...")); // #2
        Assert.That(log, Does.Contain("Welcome async (no topic), Ira!")); // #3
    }

    [Test]
    public void SubscribeType_Started_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("my-subscriber");

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        // Act
        subscriber.Start();
        var ex = Assert.Throws<InvalidOperationException>(() => subscriber.Subscribe(typeof(WelcomeAsyncHandler)));

        // Assert
        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Subscribe'. Worker state is 'Running'. Worker name is 'my-subscriber'."));
    }

    [Test]
    public void SubscribeType_Disposed_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("my-subscriber");

        subscriber.Subscribe(typeof(HelloAsyncHandler));

        // Act
        subscriber.Dispose();
        var ex = Assert.Throws<ObjectDisposedException>(() => subscriber.Subscribe(typeof(WelcomeAsyncHandler)));

        // Assert
        Assert.That(ex.ObjectName, Is.EqualTo("my-subscriber"));
    }

    #endregion

    #region Subscribe(Type, string)

    // todo clean
    //[Test]
    //public async Task SubscribeTypeString_SingleSyncHandler_HandlesMessagesWithProperTopic()
    //{
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "topic1");
    //    subscriber.Subscribe(typeof(HelloHandler), "topic2");

    //    subscriber.Start();

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    var message = new HelloMessage("Lesia")
    //    {
    //        Topic = "topic2",
    //    };
    //    publisher.Publish(message);

    //    await Task.Delay(1300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'topic2'), Lesia!"));

    //    Assert.That(log, Does.Not.Contain("Hello sync (topic: 'topic1'), Lesia!"));
    //}

    //[Test]
    //public async Task SubscribeTypeString_MultipleSyncHandlers_HandleMessagesWithProperTopic()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "topic1");

    //    subscriber.Subscribe(typeof(HelloHandler), "topic2");
    //    subscriber.Subscribe(typeof(WelcomeHandler), "topic2");

    //    subscriber.Start();

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    var message = new HelloMessage("Lesia")
    //    {
    //        Topic = "topic2",
    //    };
    //    publisher.Publish(message);

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'topic2'), Lesia!"));
    //    Assert.That(log, Does.Contain("Welcome sync (topic: 'topic2'), Lesia!"));

    //    Assert.That(log, Does.Not.Contain("Hello sync (topic: 'topic1'), Lesia!"));
    //}

    [Test]
    public async Task SubscribeTypeString_SingleAsyncHandler_HandlesMessagesWithProperTopic()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler), "topic1");
        subscriber.Subscribe(typeof(HelloAsyncHandler), "topic2");

        subscriber.Start();

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        // Act
        var message = new HelloMessage("Lesia")
        {
            Topic = "topic2",
        };
        publisher.Publish(message);

        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Hello async (topic: 'topic2'), Lesia!"));

        Assert.That(log, Does.Not.Contain("Hello async (topic: 'topic1'), Lesia!"));
    }

    [Test]
    public async Task SubscribeTypeString_MultipleAsyncHandlers_HandleMessagesWithProperTopic()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler), "topic1");
        subscriber.Subscribe(typeof(WelcomeAsyncHandler), "topic2");
        subscriber.Subscribe(typeof(HelloAsyncHandler), "topic2");

        subscriber.Start();

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        // Act
        var message = new HelloMessage("Lesia")
        {
            Topic = "topic2",
        };
        publisher.Publish(message);

        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Hello async (topic: 'topic2'), Lesia!"));
        Assert.That(log, Does.Contain("Welcome async (topic: 'topic2'), Lesia!"));

        Assert.That(log, Does.Not.Contain("Hello async (topic: 'topic1'), Lesia!"));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void SubscribeTypeString_TopicIsNullOrEmpty_ThrowsException(string badTopic)
    {
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(HelloAsyncHandler), badTopic));

        // Assert
        Assert.That(ex, Has.Message.EqualTo("'topic' cannot be null or empty. If you need a topicless subscription, use the 'Subscribe(Type messageHandlerType)' overload. (Parameter 'topic')"));
        Assert.That(ex.ParamName, Is.EqualTo("topic"));
    }

    [Test]
    public void SubscribeTypeString_TypeIsNull_ThrowsException()
    {
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => subscriber.Subscribe(null, "some-topic"));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    }

    [Test]
    public void SubscribeTypeString_TypeIsAbstract_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(AbstractHandler), "my-topic"));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' cannot be abstract."));
    }

    [Test]
    public void SubscribeTypeString_TypeIsNotClass_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(StructHandler), "my-topic"));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must represent a class."));
    }

    //[Test]
    //[TestCase(typeof(NonGenericHandler))]
    //[TestCase(typeof(NotImplementingHandlerInterface))]
    //public void SubscribeTypeString_TypeIsNotGenericSyncOrAsyncHandler_ThrowsException(Type badHandlerType)
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(badHandlerType));

    //    // Assert
    //    Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    //    Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must implement either 'IMessageHandler<TMessage>' or 'IAsyncMessageHandler<TMessage>' in a one-time manner."));
    //}

    // todo clean
    //[Test]
    //public void SubscribeTypeString_TypeIsSyncAfterAsyncSameTopic_ThrowsException()
    //{
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

    //    // Act
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloHandler), "some-topic"));

    //    // Assert
    //    Assert.That(ex, Has.Message.EqualTo($"Cannot subscribe synchronous handler '{typeof(HelloHandler)}' to message '{typeof(HelloMessage).FullName}' (topic: 'some-topic') because there are asynchronous handlers existing for that subscription."));
    //}

    // todo clean
    //[Test]
    //public async Task SubscribeTypeString_TypeIsSyncAfterAsyncButThatAsyncHasDifferentTopic_RunsOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloHandler), "another-topic");
    //    subscriber.Start();

    //    publisher.Publish(new HelloMessage("Nika")
    //    {
    //        Topic = "another-topic",
    //    });

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'another-topic'), Nika!"));
    //    Assert.That(log, Does.Not.Contain("Hello async (topic: 'another-topic'), Nika!"));
    //}

    //[Test]
    //public async Task SubscribeTypeString_TypeIsSyncAfterAsyncButThatAsyncIsTopicless_RunsOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloAsyncHandler));

    //    using var publisher = new TestMessagePublisher(_media, _logger);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloHandler), "another-topic");
    //    subscriber.Start();

    //    publisher.Publish(new HelloMessage("Nika")
    //    {
    //        Topic = "another-topic",
    //    });

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'another-topic'), Nika!"));
    //    Assert.That(log, Does.Contain("Hello async (topic: 'another-topic'), Nika!"));
    //}

    //[Test]
    //public void SubscribeTypeString_TypeIsAsyncAfterSyncSameTopic_ThrowsException()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "some-topic");

    //    // Act
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic"));

    //    // Assert
    //    Assert.That(ex, Has.Message.StartWith($"Cannot subscribe asynchronous handler '{typeof(HelloAsyncHandler).FullName}' to message '{typeof(HelloMessage).FullName}' (topic: 'some-topic') because there are synchronous handlers existing for that subscription."));
    //}

    //[Test]
    //public async Task SubscribeTypeString_TypeIsAsyncAfterSyncButThatSyncHasDifferentTopic_RunsOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "some-topic");

    //    using var publisher = new TestMessagePublisher(_media, _logger);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloAsyncHandler), "another-topic");
    //    subscriber.Start();

    //    publisher.Publish(new HelloMessage("Nika")
    //    {
    //        Topic = "another-topic",
    //    });

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello async (topic: 'another-topic'), Nika!"));
    //    Assert.That(log, Does.Not.Contain("Hello sync (topic: 'another-topic'), Nika!"));
    //}

    //[Test]
    //public async Task SubscribeTypeString_TypeIsAsyncAfterSyncButThatSyncIsTopicless_RunsOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler));

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloAsyncHandler), "another-topic");
    //    subscriber.Start();

    //    publisher.Publish(new HelloMessage("Nika")
    //    {
    //        Topic = "another-topic",
    //    });

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello async (topic: 'another-topic'), Nika!"));
    //    Assert.That(log, Does.Contain("Hello sync (topic: 'another-topic'), Nika!"));
    //}

    [Test]
    public void SubscribeTypeString_TypeImplementsIMessageHandlerTMessageMoreThanOnce_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(HelloAndByeHandler), "some-topic"));

        // Assert
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must implement 'TauCode.Mq.IMessageHandler<TMessage>' once."));
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    }

    [Test]
    public void SubscribeTypeString_TypeImplementsIAsyncMessageHandlerTMessageMoreThanOnce_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(HelloAndByeAsyncHandler), "some-topic"));

        // Assert
        Assert.That(ex, Has.Message.StartWith("'messageHandlerType' must implement 'TauCode.Mq.IMessageHandler<TMessage>' once."));
        Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    }

    //[Test]
    //public void SubscribeTypeString_TypeImplementsBothSyncAndAsync_ThrowsException()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(typeof(BothSyncAndAsyncHandler), "some-topic"));

    //    // Assert
    //    Assert.That(ex, Has.Message.StartWith($"'messageHandlerType' must implement either 'IMessageHandler<TMessage>' or 'IAsyncMessageHandler<TMessage>' in a one-time manner."));
    //    Assert.That(ex.ParamName, Is.EqualTo("messageHandlerType"));
    //}


    //[Test]
    //public void SubscribeTypeString_SyncTypeAlreadySubscribedToTheSameTopic_ThrowsException()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "some-topic");

    //    // Act
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloHandler), "some-topic"));

    //    // Assert
    //    Assert.That(ex, Has.Message.StartWith($"Handler type '{typeof(HelloHandler).FullName}' already registered for message type '{typeof(HelloMessage).FullName}' (topic: 'some-topic')."));
    //}

    //[Test]
    //public async Task SubscribeTypeString_SyncTypeAlreadySubscribedButToDifferentTopic_RunsOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "some-topic");

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloHandler), "another-topic");
    //    subscriber.Start();

    //    publisher.Publish(new HelloMessage("Alina")
    //    {
    //        Topic = "another-topic",
    //    });

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;
    //    Assert.That(log, Does.Contain("Hello sync (topic: 'another-topic'), Alina!"));
    //    Assert.That(log, Does.Not.Contain("Hello sync (topic: 'some-topic'), Alina!"));
    //}

    //[Test]
    //public async Task SubscribeTypeString_SyncTypeAlreadySubscribedButWithoutTopic_RunsOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler)); // without topic

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

    //    subscriber.Start();

    //    var message = new HelloMessage("Marina")
    //    {
    //        Topic = "some-topic",
    //    };
    //    publisher.Publish(message);

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;

    //    Assert.That(log, Does.Contain("Hello sync (topic: 'some-topic'), Marina!"));
    //    Assert.That(log, Does.Contain("Hello async (topic: 'some-topic'), Marina!"));
    //}

    //[Test]
    //public void SubscribeTypeString_AsyncTypeAlreadySubscribedToTheSameTopic_ThrowsException()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

    //    // Act
    //    var ex = Assert.Throws<MqException>(() => subscriber.Subscribe(typeof(HelloHandler), "some-topic"));

    //    // Assert
    //    Assert.That(ex, Has.Message.EqualTo($"Cannot subscribe synchronous handler '{typeof(HelloHandler).FullName}' to message '{typeof(HelloMessage).FullName}' (topic: 'some-topic') because there are asynchronous handlers existing for that subscription."));
    //}

    [Test]
    public async Task SubscribeTypeString_AsyncTypeAlreadySubscribedButToDifferentTopic_SubscribesOk()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        // Act
        subscriber.Subscribe(typeof(HelloAsyncHandler), "another-topic");
        subscriber.Start();

        publisher.Publish(new HelloMessage("Alina")
        {
            Topic = "another-topic",
        });

        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Hello async (topic: 'another-topic'), Alina!"));
        Assert.That(log, Does.Not.Contain("Hello async (topic: 'some-topic'), Alina!"));
    }

    //[Test]
    //public async Task SubscribeTypeString_AsyncTypeAlreadySubscribedButWithoutTopic_SubscribesOk()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloAsyncHandler)); // without topic

    //    using var publisher = new TestMessagePublisher(_media, _logger);
    //    publisher.Start();

    //    // Act
    //    subscriber.Subscribe(typeof(HelloHandler), "another-topic");
    //    subscriber.Start();

    //    publisher.Publish(new HelloMessage("Alina")
    //    {
    //        Topic = "another-topic",
    //    });

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;
    //    Assert.That(log, Does.Contain("Hello sync (topic: 'another-topic'), Alina!"));
    //    Assert.That(log, Does.Contain("Hello async (topic: 'another-topic'), Alina!"));
    //}

    [Test]
    [TestCase(typeof(AbstractMessageHandler))]
    [TestCase(typeof(AbstractMessageAsyncHandler))]
    public void SubscribeTypeString_TMessageIsAbstract_ThrowsException(Type badHandlerType)
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(badHandlerType, "some-topic"));

        // Assert
        Assert.That(ex, Has.Message.StartWith($"Cannot handle abstract message type '{typeof(AbstractMessage).FullName}'."));
        Assert.That(ex.ParamName, Is.EqualTo("messageType"));
    }

    //[Test]
    //[TestCase(typeof(StructMessageHandler))]
    //[TestCase(typeof(StructMessageAsyncHandler))]
    //public void SubscribeTypeString_TMessageIsNotClass_ThrowsException(Type badHandlerType)
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    // Act
    //    var ex = Assert.Throws<ArgumentException>(() => subscriber.Subscribe(badHandlerType), "some-topic");

    //    // Assert
    //    Assert.That(ex, Has.Message.StartWith($"Cannot handle non-class message type '{typeof(StructMessage).FullName}'."));
    //    Assert.That(ex.ParamName, Is.EqualTo("messageType"));
    //}

    [Test]
    public async Task SubscribeTypeString_TMessageCtorThrows_LogsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        var message = new DecayingMessage
        {
            DecayedProperty = "fresh",
            Topic = "some-topic",
        };

        DecayingMessage.IsCtorDecayed = true;

        subscriber.Subscribe(typeof(DecayingMessageHandler), "some-topic");
        subscriber.Start();

        // Act
        publisher.Publish(message);
        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Alas Ctor Decayed!"));
    }

    [Test]
    public async Task SubscribeTypeString_TMessagePropertyThrows_LogsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        var message = new DecayingMessage
        {
            DecayedProperty = "fresh",
            Topic = "some-topic",
        };

        DecayingMessage.IsPropertyDecayed = true;

        subscriber.Subscribe(typeof(DecayingMessageHandler), "some-topic");
        subscriber.Start();

        // Act
        _media.Publish(message);
        await Task.Delay(300);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Alas Property Decayed!"));
    }


    // todo: review ut-s of entire 'Bundle.Handle', 'Bundle.AsyncHandle' loops. If an exception was thrown at any step, we must give the chance to other handlers. 

    //[Test]
    //public async Task SubscribeTypeString_SyncHandlerThrows_RestDoRun()
    //{
    //    // Arrange
    //    using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

    //    subscriber.Subscribe(typeof(HelloHandler), "some-topic"); // #0 will handle
    //    subscriber.Subscribe(typeof(FishHaterHandler), "some-topic"); // #1 will fail
    //    subscriber.Subscribe(typeof(WelcomeHandler), "some-topic"); // #2 will handle

    //    subscriber.Start();

    //    using var publisher = new TestMessagePublisher(_media);
    //    publisher.Start();

    //    var message = new HelloMessage("Small Fish")
    //    {
    //        Topic = "some-topic",
    //    };

    //    // Act
    //    publisher.Publish(message);

    //    await Task.Delay(300);

    //    // Assert
    //    var log = this.CurrentLog;
    //    Assert.That(log, Does.Contain("Hello sync (topic: 'some-topic'), Small Fish!"));
    //    Assert.That(log, Does.Contain("I hate you sync (topic: 'some-topic'), 'Small Fish'! Exception thrown!"));
    //    Assert.That(log, Does.Contain("Welcome sync (topic: 'some-topic'), Small Fish!"));
    //}

    // todo: when topic is present, topicless subscription does not fire (sync or async) - [2021-03-31] Is it so? I think now that vice versa


    // todo - async handler's HandleAsync is faulted => logs, stops loop gracefully.
    [Test]
    public async Task SubscribeTypeString_AsyncHandlerFaulted_LogsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(FaultingHelloAsyncHandler), "some-topic");
        subscriber.Start();

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        // Act
        publisher.Publish(new HelloMessage("Ania")
        {
            Topic = "some-topic",
        });

        await Task.Delay(500);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain("Sorry, I am faulting async (topic: 'some-topic'), Ania..."));
        Assert.That(log, Does.Not.Contain("Context ended."));
    }

    // todo: sync handler throwing => rest of them working.

    [Test]
    public async Task SubscribeTypeString_AsyncHandlerCanceledOrFaulted_RestDoRun()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic"); // #0 will say 'hello'
        subscriber.Subscribe(typeof(CancelingHelloAsyncHandler), "some-topic"); // #1 will cancel with message
        subscriber.Subscribe(typeof(FaultingHelloAsyncHandler), "some-topic"); // #2 will fault with message
        subscriber.Subscribe(typeof(WelcomeAsyncHandler), "some-topic"); // #3 will say 'welcome', regardless of #1 canceled and #2 faulted.

        subscriber.Start();

        using var publisher = new TestMessagePublisher(_media, _logger);
        publisher.Start();

        // Act
        publisher.Publish(new HelloMessage("Ira")
        {
            Topic = "some-topic",
        });

        await Task.Delay(500);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Hello async (topic: 'some-topic'), Ira!")); // #0
        Assert.That(log, Does.Contain("Sorry, I am cancelling async (topic: 'some-topic'), Ira...")); // #1
        Assert.That(log, Does.Contain("Sorry, I am faulting async (topic: 'some-topic'), Ira...")); // #2
        Assert.That(log, Does.Contain("Welcome async (topic: 'some-topic'), Ira!")); // #3
    }

    [Test]
    public void SubscribeTypeString_Started_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("my-subscriber");

        subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

        // Act
        subscriber.Start();
        var ex = Assert.Throws<InvalidOperationException>(() => subscriber.Subscribe(typeof(WelcomeAsyncHandler), "some-topic"));

        // Assert
        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Subscribe'. Worker state is 'Running'. Worker name is 'my-subscriber'."));

    }

    [Test]
    public void SubscribeTypeString_Disposed_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("my-subscriber");

        subscriber.Subscribe(typeof(HelloAsyncHandler), "some-topic");

        // Act
        subscriber.Dispose();
        var ex = Assert.Throws<ObjectDisposedException>(() => subscriber.Subscribe(typeof(WelcomeAsyncHandler), "some-topic"));

        // Assert
        Assert.That(ex.ObjectName, Is.EqualTo("my-subscriber"));
    }

    #endregion

    #region GetSubscriptions()

    [Test]
    public void GetSubscriptions_JustCreated_ReturnsEmptyArray()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        var subscriptions = subscriber.GetSubscriptions();

        // Assert
        Assert.That(subscriptions, Is.Empty);
    }

    [Test]
    public void GetSubscriptions_Running_ReturnsSubscriptions()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Subscribe(typeof(WelcomeAsyncHandler));

        subscriber.Subscribe(typeof(ByeAsyncHandler));

        subscriber.Start();

        // Act
        var subscriptions = subscriber.GetSubscriptions();

        // Assert
        Assert.That(subscriptions, Has.Count.EqualTo(2));

        var info0 = subscriptions[0];
        Assert.That(info0.MessageType, Is.EqualTo(typeof(HelloMessage)));
        Assert.That(info0.Topic, Is.Null);
        CollectionAssert.AreEqual(
            new[] { typeof(HelloAsyncHandler), typeof(WelcomeAsyncHandler) },
            info0.MessageHandlerTypes);

        var info1 = subscriptions[1];
        Assert.That(info1.MessageType, Is.EqualTo(typeof(ByeMessage)));
        Assert.That(info1.Topic, Is.Null);
        CollectionAssert.AreEqual(
            new[] { typeof(ByeAsyncHandler), },
            info1.MessageHandlerTypes);
    }

    [Test]
    public void GetSubscriptions_Stopped_ReturnsSubscriptions()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Subscribe(typeof(WelcomeAsyncHandler));

        subscriber.Subscribe(typeof(ByeAsyncHandler));

        subscriber.Start();
        subscriber.Stop();

        // Act
        var subscriptions = subscriber.GetSubscriptions();

        // Assert
        Assert.That(subscriptions, Has.Count.EqualTo(2));

        var info0 = subscriptions[0];
        Assert.That(info0.MessageType, Is.EqualTo(typeof(HelloMessage)));
        Assert.That(info0.Topic, Is.Null);
        CollectionAssert.AreEqual(
            new[] { typeof(HelloAsyncHandler), typeof(WelcomeAsyncHandler) },
            info0.MessageHandlerTypes);

        var info1 = subscriptions[1];
        Assert.That(info1.MessageType, Is.EqualTo(typeof(ByeMessage)));
        Assert.That(info1.Topic, Is.Null);
        CollectionAssert.AreEqual(
            new[] { typeof(ByeAsyncHandler), },
            info1.MessageHandlerTypes);
    }

    [Test]
    public async Task GetSubscriptions_Disposed_ReturnsEmptyArray()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Subscribe(typeof(WelcomeAsyncHandler));

        subscriber.Start();

        // Act
        subscriber.Dispose();
        await Task.Delay(200);

        var subscriptions = subscriber.GetSubscriptions();

        // Assert
        Assert.That(subscriptions, Is.Empty);
    }

    #endregion

    #region Name

    [Test]
    public void Name_NotDisposed_IsChangedAndCanBeRead()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub_created");

        // Act
        var nameCreated = subscriber.Name;

        subscriber.Start();
        subscriber.Name = "sub_started";

        var nameStarted = subscriber.Name;

        subscriber.Stop();
        subscriber.Name = "sub_stopped";

        var nameStopped = subscriber.Name;

        // Assert
        Assert.That(nameCreated, Is.EqualTo("sub_created"));
        Assert.That(nameStarted, Is.EqualTo("sub_started"));
        Assert.That(nameStopped, Is.EqualTo("sub_stopped"));
    }

    [Test]
    public void Name_Disposed_CanOnlyBeRead()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("name1");

        // Act
        subscriber.Dispose();

        var name = subscriber.Name;
        var ex = Assert.Throws<ObjectDisposedException>(() => subscriber.Name = "name2");

        // Assert
        Assert.That(name, Is.EqualTo("name1"));
        Assert.That(subscriber.Name, Is.EqualTo("name1"));
        Assert.That(ex.ObjectName, Is.EqualTo("name1"));
    }

    #endregion

    #region State

    [Test]
    public void State_JustCreated_EqualsToStopped()
    {
        // Arrange

        // Act
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Stopped));
    }

    [Test]
    public void State_Started_EqualsToStarted()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        subscriber.Start();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Running));
    }

    [Test]
    public void State_Stopped_EqualsToStopped()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Start();

        // Act
        subscriber.Stop();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Stopped));
    }

    [Test]
    public void State_DisposedJustAfterCreation_EqualsToStopped()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Stopped));
    }

    [Test]
    public void State_DisposedAfterStarted_EqualsToStopped()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Start();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Stopped));
    }

    [Test]
    public void State_DisposedAfterStopped_EqualsToStopped()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Start();
        subscriber.Stop();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Stopped));
    }

    [Test]
    public void State_DisposedAfterDisposed_EqualsToStopped()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Start();
        subscriber.Stop();
        subscriber.Dispose();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.State, Is.EqualTo(WorkerState.Stopped));
    }

    #endregion

    #region IsDisposed

    [Test]
    public void IsDisposed_JustCreated_EqualsToFalse()
    {
        // Arrange

        // Act
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        // Assert
        Assert.That(subscriber.IsDisposed, Is.False);
    }

    [Test]
    public void IsDisposed_Started_EqualsToFalse()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        // Act
        subscriber.Start();

        // Assert
        Assert.That(subscriber.IsDisposed, Is.False);
    }

    [Test]
    public void IsDisposed_Stopped_EqualsToFalse()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Start();

        // Act
        subscriber.Stop();

        // Assert
        Assert.That(subscriber.IsDisposed, Is.False);
    }

    [Test]
    public void IsDisposed_DisposedJustAfterCreation_EqualsToTrue()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.IsDisposed, Is.True);
    }

    [Test]
    public void IsDisposed_DisposedAfterStarted_EqualsToTrue()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Start();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.IsDisposed, Is.True);
    }

    [Test]
    public void IsDisposed_DisposedAfterStopped_EqualsToTrue()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Start();
        subscriber.Stop();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.IsDisposed, Is.True);
    }

    [Test]
    public void IsDisposed_DisposedAfterDisposed_EqualsToTrue()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Dispose();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.That(subscriber.IsDisposed, Is.True);
    }

    #endregion

    #region Start()

    [Test]
    public async Task Start_JustCreated_StartsAndHandlesMessages()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Subscribe(typeof(WelcomeAsyncHandler));

        subscriber.Subscribe(typeof(ByeAsyncHandler));
        subscriber.Subscribe(typeof(BeBackAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(new HelloMessage("Ira"));
        _media.Publish(new ByeMessage("Olia"));

        await Task.Delay(200);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Bye async (no topic), Olia!"));
        Assert.That(log, Does.Contain("Be back async (no topic), Olia!"));
    }

    [Test]
    public void Start_Started_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("my-subscriber");

        subscriber.Start();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => subscriber.Start());

        // Assert
        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Start'. Worker state is 'Running'. Worker name is 'my-subscriber'."));
    }

    [Test]
    public async Task Start_Stopped_StartsAndHandlesMessages()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Subscribe(typeof(WelcomeAsyncHandler));

        subscriber.Subscribe(typeof(ByeAsyncHandler));
        subscriber.Subscribe(typeof(BeBackAsyncHandler));

        subscriber.Start();

        // Act
        _media.Publish(new HelloMessage("Ira"));
        _media.Publish(new ByeMessage("Olia"));

        await Task.Delay(200);

        subscriber.Stop();
        await Task.Delay(200);

        subscriber.Start();

        _media.Publish(new HelloMessage("Manuela"));
        _media.Publish(new ByeMessage("Alex"));

        await Task.Delay(200);

        // Assert
        var log = this.CurrentLog;

        Assert.That(log, Does.Contain("Bye async (no topic), Alex!"));
        Assert.That(log, Does.Contain("Be back async (no topic), Alex!"));
    }

    [Test]
    public void Start_Disposed_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("my-subscriber");

        subscriber.Dispose();

        // Act
        var ex = Assert.Throws<ObjectDisposedException>(() => subscriber.Start());

        // Assert
        Assert.That(ex.ObjectName, Is.EqualTo("my-subscriber"));
    }

    #endregion

    #region Stop()

    [Test]
    public void Stop_JustCreated_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => subscriber.Stop());

        // Assert
        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'sub'."));
    }

    [Test]
    public async Task Stop_Started_StopsAndCancelsCurrentAsyncTasks()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Start();

        _media.Publish(new HelloMessage()
        {
            Name = "Koika",
            MillisecondsTimeout = 3000,
        });

        // Act
        await Task.Delay(500); // let 'HelloAsyncHandler' start.

        subscriber.Stop(); // should cancel 'HelloAsyncHandler'

        await Task.Delay(100);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain($"A task was canceled."));
    }

    [Test]
    public void Stop_Stopped_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Start();
        subscriber.Stop();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => subscriber.Stop());

        // Assert
        Assert.That(ex, Has.Message.EqualTo("Cannot perform operation 'Stop'. Worker state is 'Stopped'. Worker name is 'sub'."));
    }

    [Test]
    public void Stop_Disposed_ThrowsException()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>("sub");

        subscriber.Dispose();

        // Act
        var ex = Assert.Throws<ObjectDisposedException>(() => subscriber.Stop());

        // Assert
        Assert.That(ex.ObjectName, Is.EqualTo("sub"));
    }

    #endregion

    #region Dispose()

    [Test]
    public void Dispose_JustCreated_Disposes()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.Pass("Test passed.");
    }

    [Test]
    public async Task Dispose_Started_DisposesAndCancelsCurrentAsyncTasks()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();
        //subscriber.Logger = _logger;

        subscriber.Subscribe(typeof(HelloAsyncHandler));
        subscriber.Start();

        _media.Publish(new HelloMessage()
        {
            Name = "Koika",
            MillisecondsTimeout = 3000,
        });

        // Act
        await Task.Delay(200); // let 'HelloAsyncHandler' start.

        subscriber.Dispose(); // should cancel 'HelloAsyncHandler'

        await Task.Delay(100);

        // Assert
        var log = this.CurrentLog;
        Assert.That(log, Does.Contain($"A task was canceled."));
    }

    [Test]
    public void Dispose_Stopped_Disposes()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();
        subscriber.Start();
        subscriber.Stop();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.Pass("Test passed.");
    }

    [Test]
    public void Dispose_Disposed_DoesNothing()
    {
        // Arrange
        using var subscriber = this.CreateMessageSubscriber<GoodContextFactory>();
        subscriber.Dispose();

        // Act
        subscriber.Dispose();

        // Assert
        Assert.Pass("Test passed.");
    }

    #endregion
}