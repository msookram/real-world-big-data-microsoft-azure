using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;

namespace Telemetry.DeepStorage.Worker.Tests.Unit.Storage
{
    [TestClass]
    public class BlobStorageEventStoreTests
    {   
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            AzureEmulator.StartStorage();
        }

        [TestInitialize]
        public void Setup()
        {
            AzureEmulator.ResetContainer(ConfigurationManager.AppSettings["DeepStorage.OutputContainerName"]);
        }

        [TestMethod]
        public void Write_1MB_Commits_1Block()
        {
            var store = new BlobStorageEventStore();
            store.Initialise("0", "2015020309");                     
            
            WriteFakeEvents(store, 25, 40 * 1024);

            var blob = GetBlockBlobReference(store.Path);
            var blockList = blob.DownloadBlockList();
            Assert.AreEqual(1, blockList.Count());
        }

        [TestMethod]
        public void Write_4MB_Commits_1Block()
        {
            var store = new BlobStorageEventStore();
            store.Initialise("0", "2015020309");

            WriteFakeEvents(store, 100, 40 * 1024);

            var blob = GetBlockBlobReference(store.Path);
            var blockList = blob.DownloadBlockList();
            Assert.AreEqual(1, blockList.Count());
        }

        [TestMethod]
        public void Write_20MB_Commits_5Blocks()
        {
            var store = new BlobStorageEventStore();
            store.Initialise("0", "2015020309");

            WriteFakeEvents(store, 500, 40 * 1024);

            var blob = GetBlockBlobReference(store.Path);
            var blockList = blob.DownloadBlockList();
            Assert.AreEqual(5, blockList.Count());
        }

        private static void WriteFakeEvents(IEventStore store, int eventCount, int payloadSize)
        {
            var bytes = new List<byte>();
            for (int i = 0; i < eventCount; i++)
            {                
                for (int j = 0; j < payloadSize; j++)
                {
                    bytes.AddRange(Encoding.UTF8.GetBytes("a"));
                }                
            }
            store.Write(bytes.ToArray());
        }

        private static CloudBlockBlob GetBlockBlobReference(string blobName)
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["DeepStorage.OutputConnectionString"]);
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["DeepStorage.OutputContainerName"]);
            return container.GetBlockBlobReference(blobName);
        }
    }
}
