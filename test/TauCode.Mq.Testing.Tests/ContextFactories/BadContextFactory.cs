using System;
using TauCode.Mq.Testing.Tests.Contexts;

namespace TauCode.Mq.Testing.Tests.ContextFactories
{
    public class BadContextFactory : IMessageHandlerContextFactory
    {
        private readonly bool _throwsOnCreateContext;
        private readonly bool _returnsNullOnCreateContext;

        private readonly bool _contextThrowsOnBegin;
        private readonly bool _contextThrowsOnEnd;

        private readonly bool _contextThrowsOnGetService;
        private readonly bool _contextReturnsNullOnGetService;
        private readonly bool _contextReturnsWrongServiceOnGetService;

        private readonly bool _contextThrowsOnDispose;

        public BadContextFactory(
            bool throwsOnCreateContext,
            bool returnsNullOnCreateContext,
            bool contextThrowsOnBegin,
            bool contextThrowsOnEnd,
            bool contextThrowsOnGetService,
            bool contextReturnsNullOnGetService,
            bool contextReturnsWrongServiceOnGetService,
            bool contextThrowsOnDispose)
        {
            _throwsOnCreateContext = throwsOnCreateContext;
            _returnsNullOnCreateContext = returnsNullOnCreateContext;

            _contextThrowsOnBegin = contextThrowsOnBegin;
            _contextThrowsOnEnd = contextThrowsOnEnd;

            _contextThrowsOnGetService = contextThrowsOnGetService;
            _contextReturnsNullOnGetService = contextReturnsNullOnGetService;
            _contextReturnsWrongServiceOnGetService = contextReturnsWrongServiceOnGetService;

            _contextThrowsOnDispose = contextThrowsOnDispose;
        }

        public IMessageHandlerContext CreateContext()
        {
            if (_throwsOnCreateContext)
            {
                throw new NotSupportedException("Failed to create context.");
            }

            if (_returnsNullOnCreateContext)
            {
                return null;
            }

            return new BadContext(
                _contextThrowsOnBegin,
                _contextThrowsOnEnd,
                _contextThrowsOnGetService,
                _contextReturnsNullOnGetService,
                _contextReturnsWrongServiceOnGetService,
                _contextThrowsOnDispose);
        }
    }
}
