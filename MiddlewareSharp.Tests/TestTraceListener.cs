using System.Diagnostics;
using Xunit.Abstractions;

namespace MiddlewareSharp.Tests
{
    public class TestTraceListener : TraceListener
    {
        private readonly ITestOutputHelper _output;

        public TestTraceListener(ITestOutputHelper testOutput)
        {
            _output = testOutput;
        }

        public override void Write(string message)
        {
            _output.WriteLine(message);
        }

        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
    }
}
