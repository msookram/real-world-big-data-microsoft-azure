using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.EventProcessor.Pig.CountLogger
{
    class ConfigSetup
    {
        public static void WriteConfigResources()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            WriteConfigFileFromResource(thisAssembly, "App.config", "Telemetry.EventProcessor.Pig.CountLogger.exe.config");
            WriteConfigFileFromResource(thisAssembly, "nlog-prd.config", "nlog-prd.config");
        }

        private static void WriteConfigFileFromResource(Assembly assembly, string resourceName, string outputFileName)
        {
            if (!File.Exists(outputFileName))
            {
                var fullName = string.Format("{0}.{1}", assembly.GetName().Name, resourceName);
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(fullName)))
                {
                    var content = reader.ReadToEnd();
                    File.WriteAllText(outputFileName, content);

                    Console.WriteLine("** Wrote file: " + outputFileName);
                }
            }
        }
    }
}
