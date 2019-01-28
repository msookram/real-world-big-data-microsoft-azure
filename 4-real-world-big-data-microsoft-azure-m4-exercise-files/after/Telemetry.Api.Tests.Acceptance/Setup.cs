using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using Telemetry.Api.Tests.Stubs.EventIngestionApi;

namespace Telemetry.Api.Tests.Acceptance
{
    [TestClass]
    public sealed class Setup
    {
        [AssemblyInitialize]
        public static void Start(TestContext context)
        {
            SelfHost.Start();
            if (SelfHost.ServerStartException != null)
            {
                throw (SelfHost.ServerStartException);
            }
            Thread.Sleep(500);
        }

        [AssemblyCleanup]
        public static void Stop()
        {
            SelfHost.Stop();
        }
    }
}
