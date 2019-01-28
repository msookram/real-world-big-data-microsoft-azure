using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Api.Tests.Stubs.EventIngestionApi
{
    class Program
    {
        static void Main(string[] args)
        {
            SelfHost.Start();
            if (SelfHost.ServerStartException != null)
            {
                throw (SelfHost.ServerStartException);
            }
            Console.WriteLine("Stub listening on port 8888");
            Console.ReadLine();
        }
    }
}