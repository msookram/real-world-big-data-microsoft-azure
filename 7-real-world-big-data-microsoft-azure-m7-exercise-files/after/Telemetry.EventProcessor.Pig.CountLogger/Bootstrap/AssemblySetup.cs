using System;
using System.IO;
using System.Reflection;

namespace Telemetry.EventProcessor.Pig.CountLogger
{
    public static class AssemblySetup
    {
        public static void LoadAssemblyResources()
        {
            var dependencies = new[] 
            { 
                //"Microsoft.Practices.Unity.Configuration.dll", 
                //"Microsoft.Practices.Unity.dll",
                //"Microsoft.ServiceBus.dll",
                //"Microsoft.WindowsAzure.Configuration.dll",
                //"Microsoft.WindowsAzure.Storage.dll",
                "NLog.dll",
                "Microsoft.WindowsAzure.Configuration.dll",
                "Telemetry.Core.dll"
            };

            var thisAssembly = Assembly.GetExecutingAssembly();
            foreach (var dependency in dependencies)
            {
                LoadAssemblyFromResource(thisAssembly, dependency);
                Assembly.LoadFile(dependency);
            }
        }

        private static void LoadAssemblyFromResource(Assembly thisAssembly, string resourceName)
        {
            var fullName = string.Format("{0}.Assemblies.{1}", Assembly.GetExecutingAssembly().GetName().Name, resourceName);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName))
            {
                var assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                File.WriteAllBytes(resourceName, assemblyData);
                //Assembly.Load(assemblyData);

                Console.WriteLine("** Wrote file: " + resourceName);
            }
        }
    }
}
