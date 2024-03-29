﻿using Serilog;

namespace TauCode.Mq.Testing;

public class TestMessageSubscriber : MessageSubscriberBase
{
    #region Fields

    private readonly ITestMqMedia _media;

    #endregion

    #region Constructor

    public TestMessageSubscriber(IMessageHandlerContextFactory contextFactory, ITestMqMedia media, ILogger logger)
        : base(contextFactory, logger)
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
    }

    protected override IDisposable SubscribeImpl(ISubscriptionRequest subscriptionRequest)
    {
        // todo clean
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

        //if (subscriptionRequest.AsyncHandler != null && subscriptionRequest.Handler == null)
        //{
        //    // got async subscription
        //    if (subscriptionRequest.Topic == null)
        //    {
        //        return _media.Subscribe(
        //            subscriptionRequest.MessageType,
        //            subscriptionRequest.AsyncHandler);
        //    }
        //    else
        //    {
        //        return _media.Subscribe(
        //            subscriptionRequest.MessageType,
        //            subscriptionRequest.AsyncHandler,
        //            subscriptionRequest.Topic);
        //    }
        //}
        //else if (subscriptionRequest.Handler != null && subscriptionRequest.AsyncHandler == null)
        //{
        //    // got sync subscription
        //    if (subscriptionRequest.Topic == null)
        //    {
        //        return _media.Subscribe(
        //            subscriptionRequest.MessageType,
        //            obj =>
        //            {
        //                subscriptionRequest.Handler(obj);
        //                return Task.CompletedTask;
        //            });
        //    }
        //    else
        //    {
        //        return _media.Subscribe(
        //            subscriptionRequest.MessageType,
        //            obj =>
        //            {
        //                subscriptionRequest.Handler(obj);
        //                return Task.CompletedTask;
        //            },
        //            subscriptionRequest.Topic);
        //    }
        //}
        //else
        //{
        //    throw new ArgumentException(
        //        $"'{nameof(subscriptionRequest)}' is invalid.",
        //        nameof(subscriptionRequest));
        //}
    }

    #endregion
}