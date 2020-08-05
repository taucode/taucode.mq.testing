using System.Text;

namespace TauCode.Mq.Testing.Tests
{
    public class DemoLog
    {
        public static DemoLog Instance { get; } = new DemoLog();

        private DemoLog()
        {
            this.StringBuilder = new StringBuilder();
        }

        public StringBuilder StringBuilder { get; }
    }
}
