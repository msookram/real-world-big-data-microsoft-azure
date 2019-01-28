using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Telemetry.DeepStorage.Worker.Tests.Unit
{
    public static class AzureEmulator
    {
        private static bool _emulatorStarted;

        public static void StartStorage()
        {
            var exitCode = RunStorageCommand("start", true);
            _emulatorStarted = (exitCode == 0);
        }

        public static void ClearStorage(bool waitForExit = true)
        {
            RunStorageCommand("clear all", waitForExit);
        }

        public static void ClearBlob(bool waitForExit = true)
        {
            RunStorageCommand("clear blob", waitForExit);
        }

        public static void StopStorage(bool waitForExit = false)
        {
            if (_emulatorStarted)
            {
                RunStorageCommand("stop", waitForExit);
            }
        }

        public static void ResetContainer(string containerName, Dictionary<string, string> metadata = null)
        {
            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true;");
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            if (container.Exists())
            {
                var blobs = container.ListBlobs(useFlatBlobListing: true);
                foreach (var blob in blobs.OfType<CloudBlockBlob>())
                {
                    blob.Delete();
                }
                if (metadata != null)
                {
                    container.FetchAttributes();
                    foreach (var item in metadata)
                    {
                        container.Metadata[item.Key] = item.Value;
                    }
                    container.SetMetadata();
                }
            }
        }

        private static int RunStorageCommand(string arguments, bool waitForExit)
        {
             var emulatorPsi = new ProcessStartInfo(ConfigurationManager.AppSettings["StorageEmulatorPath"]);
             emulatorPsi.Arguments = arguments;
             var process = Process.Start(emulatorPsi);
             if (waitForExit)
             {
                 process.WaitForExit();
                 return process.ExitCode;
             }
             return 0;
        }
    }
}
