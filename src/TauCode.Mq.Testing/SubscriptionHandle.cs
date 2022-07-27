using System;

namespace TauCode.Mq.Testing
{
    internal class SubscriptionHandle : IDisposable
    {
        #region Fields

        private readonly TestMqMedia _media;
        private readonly string _tag;
        private readonly string _id;
        private bool _isDisposed;

        #endregion

        #region Constructor

        internal SubscriptionHandle(TestMqMedia media, string tag, string id)
        {
            _media = media;
            _tag = tag;
            _id = id;
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _media.Unsubscribe(_tag, _id);
            _isDisposed = true;
        }

        #endregion
    }
}
