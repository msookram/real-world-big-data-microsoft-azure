using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telemetry.Core;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;

namespace Telemetry.DeepStorage.Worker.Tests.Unit.Storage
{
    [TestClass]
    public class DiskEventStoreTests
    {
        [TestMethod]
        public void Write_1MB_DoesNotFlush()
        {
            var mockNextStore = SetupLevel3Mock();

            var store = new DiskEventStore();
            store.Initialise("0", "2015020309");

            AssertEmptyFileCreated(store);
            
            WriteFakeEvents(store, 20);

            mockNextStore.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Never);
            Assert.IsTrue(File.Exists(store.Path));

            //should get > 50% compression:
            Assert.IsTrue((new FileInfo(store.Path)).Length / 2 < (20 * 40 * 1024));
        }

        [TestMethod]
        public void Write_2MB_FlushesOnce()
        {
            var mockNextStore = SetupLevel3Mock();

            var store = new DiskEventStore();
            store.Initialise("0", "2015020309");

            AssertEmptyFileCreated(store);

            WriteFakeEvents(store, 20);
            mockNextStore.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Never);

            WriteFakeEvents(store, 20);
            Thread.Sleep(250);

            mockNextStore.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Once);            

            Assert.IsTrue(File.Exists(store.Path));
            Assert.IsTrue((new FileInfo(store.Path)).Length / 2 < (40 * 40 * 1024));
        }

        [TestMethod]
        public void MultiThreadedWrite_2MB_FlushesOnce()
        {
            var mockNextStore = SetupLevel3Mock();

            var store = new DiskEventStore();
            store.Initialise("0", "2015020309");

            AssertEmptyFileCreated(store);

            var writeTasks = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
                writeTasks.Add(Task.Factory.StartNew(() => WriteFakeEvents(store, 14)));
            }
            Task.WaitAll(writeTasks.ToArray());
            Thread.Sleep(250);

            mockNextStore.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Once);

            Assert.IsTrue(File.Exists(store.Path));
            Assert.IsTrue((new FileInfo(store.Path)).Length / 2 < (42 * 40 * 1024));
        }

        private static void AssertEmptyFileCreated(DiskEventStore store)
        {
            Assert.IsFalse(string.IsNullOrEmpty(store.Path));
            Assert.IsTrue(store.Path.Contains("\\2015020309p0-"));
            Assert.IsTrue(store.Path.EndsWith(".json.gz"));
            Assert.IsTrue(File.Exists(store.Path));
            Assert.AreEqual(0, (new FileInfo(store.Path)).Length);
        }

        private static Mock<IEventStore> SetupLevel3Mock()
        {
            var mockStore = new Mock<IEventStore>();
            mockStore.SetupGet(x => x.Level).Returns(3);
            Container.Instance.RegisterInstance<IEventStore>("3", mockStore.Object);
            return mockStore;
        }

        private static void WriteFakeEvents(IEventStore store, int eventCount, int payloadSize = 40 * 1024)
        {
            for (int i = 0; i < eventCount; i++)
            {
                var bytes = new List<byte>();
                for (int j = 0; j < payloadSize; j++)
                {
                    var character = Guid.NewGuid().ToString().Substring(0, 1);
                    bytes.AddRange(Encoding.UTF8.GetBytes(character));
                }
                store.Write(bytes.ToArray());
            }
        }

    }
}
