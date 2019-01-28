using System;

namespace MiddlewareSharp.Tests
{
    public class TestContext
    {
        public int N { get; set; }
		public int CatchN { get; set; }
		public MiddlewareException<TestContext> CatchedException { get; set; }
    }
}
