using System.Threading;
using System.Threading.Tasks;
using TauCode.Working;

namespace TauCode.Mq.Testing
{
    internal class MessageQueue : QueueWorkerBase<MessagePackage>
    {
        private readonly TestMqMedia _media;

        internal MessageQueue(TestMqMedia media)
        {
            _media = media;
        }

        protected override async Task DoAssignment(MessagePackage assignment, CancellationToken cancellationToken)
        {
            await _media.DispatchMessagePackage(assignment);
        }
    }
}
