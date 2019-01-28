
using System;
using System.Collections.Generic;
using System.IO;

namespace Telemetry.EventProcessor.Pig.CountLogger
{
    class Program
    {       
        static Program()
        {
            //AssemblySetup.LoadAssemblyResources();
            //ConfigSetup.WriteConfigResources();
        }

        static void Main(string[] args)
        {
            EventCountLogger.Run();
        }
    }
}
