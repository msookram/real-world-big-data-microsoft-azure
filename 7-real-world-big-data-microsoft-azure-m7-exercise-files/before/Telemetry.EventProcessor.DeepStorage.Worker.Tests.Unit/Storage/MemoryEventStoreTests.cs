using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telemetry.Core;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;

namespace Telemetry.DeepStorage.Worker.Tests.Unit.Storage
{
    [TestClass]
    public class MemoryEventStoreTests
    {
        [TestMethod]
        public void Write_3MB_DoesNotFlush()
        {
            var mockLevel2Store = SetupLevel2Mock();

            var memoryStore = new MemoryEventStore();
            memoryStore.Initialise("0", "2015020309");
            
            WriteFakeEvents(memoryStore, 80, 40 * 1024);

            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        public void Write_3MB_FlushesAfterTimeout()
        {
            var mockLevel2Store = SetupLevel2Mock();

            var memoryStore = new MemoryEventStore();
            memoryStore.Initialise("0", "2015020309");

            WriteFakeEvents(memoryStore, 80, 40 * 1024);
            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Never);

            Thread.Sleep(2500);
            WriteFakeEvents(memoryStore, 1, 1 * 1024);
            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public void Write_16MB_FlushesTwice()
        {
            var mockLevel2Store = SetupLevel2Mock();

            var memoryStore = new MemoryEventStore();
            memoryStore.Initialise("0", "2015020309");

            WriteFakeEvents(memoryStore, 410, 40 * 1024);
            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Write_16MB_FlushesTwice_AndResetsTimeout()
        {
            var mockLevel2Store = SetupLevel2Mock();

            var memoryStore = new MemoryEventStore();
            memoryStore.Initialise("0", "2015020309");

            var writeStopwatch = Stopwatch.StartNew();
            WriteFakeEvents(memoryStore, 410, 40 * 1024);
            writeStopwatch.Stop();

            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Exactly(2));
            Assert.IsFalse(memoryStore.IsFlushOverdue());

            Thread.Sleep(3000);
            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Exactly(2));
            Assert.IsTrue(memoryStore.IsFlushOverdue());
        }

        [TestMethod]
        public void MultiThreadedWrite_3MB_FlushesAfterTimeout()
        {
            var mockLevel2Store = SetupLevel2Mock();

            var memoryStore = new MemoryEventStore();
            memoryStore.Initialise("0", "2015020309");
            var writeStopwatch = Stopwatch.StartNew();
            var writeTasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                writeTasks.Add(Task.Factory.StartNew(() => WriteFakeEvents(memoryStore, 8, 40 * 1024)));
            }
            Task.WaitAll(writeTasks.ToArray());

            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Never);
            Thread.Sleep(3000);
            WriteFakeEvents(memoryStore, 1, 1 * 1024);
            mockLevel2Store.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Once);
        }

        private static Mock<IEventStore> SetupLevel2Mock()
        {
            var mockLevel2Store = new Mock<IEventStore>();
            mockLevel2Store.SetupGet(x => x.Level).Returns(1);
            Container.Instance.RegisterInstance<IEventStore>("2", mockLevel2Store.Object);
            return mockLevel2Store;
        }

        private static void WriteFakeEvents(IEventStore store, int eventCount, int payloadSize)
        {
            for (int i = 0; i < eventCount; i++)
            {
                var bytes = new List<byte>();
                for (int j = 0; j < payloadSize; j++)
                {
                    bytes.AddRange(Encoding.UTF8.GetBytes("a"));
                }
                store.Write(bytes.ToArray());
            }
        }
    }
}
