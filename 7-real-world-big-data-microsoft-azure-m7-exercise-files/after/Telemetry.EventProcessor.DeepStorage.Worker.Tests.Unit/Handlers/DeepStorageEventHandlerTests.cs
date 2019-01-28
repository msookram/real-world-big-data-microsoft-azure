using System;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telemetry.Core;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Moq;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;
using Telemetry.EventProcessor.DeepStorage.Worker.EventHandlers;

namespace Telemetry.DeepStorage.Worker.Tests.Unit.Handlers
{
    [TestClass]
    public class DeepStorageEventHandlerTests
    {
        private static Random _Random = new Random();
        private static Dictionary<long, string> _ReceivedAts;

        static DeepStorageEventHandlerTests()
        {
            _ReceivedAts = new Dictionary<long, string>();
            _ReceivedAts.Add(1422964800000, "2015020312");
            _ReceivedAts.Add(1422968400000, "2015020313");
            _ReceivedAts.Add(1419498000000, "2014122509");
        }

        [TestMethod]
        public void Handle_Events()
        {
            var l1Mock = new Mock<IEventStore>();
            Container.Instance.RegisterInstance<IEventStore>("1", l1Mock.Object);

            var handler = new DeepStorageEventHandler();

            var receivedAtIndex = _Random.Next(0, 2);
            var receivedAt = _ReceivedAts.Keys.ElementAt(receivedAtIndex);
            var receivedAtHour = _ReceivedAts[receivedAt];

            SendEvents(handler, 100, 40 * 1024, receivedAt);
            l1Mock.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Exactly(100));

            SendEvents(handler, 200, 40 * 1024, receivedAt);
            l1Mock.Verify(x => x.Write(It.IsAny<byte[]>()), Times.Exactly(300));

            l1Mock.Verify(x => x.Flush(), Times.Never);
        }

        private static void SendEvents(DeepStorageEventHandler handler, int eventCount, int payloadSize, long receivedAt)
        {
            for (int i = 0; i < eventCount; i++)
            {
                var json = @"{""sourceId"" : ""1324""}";
                dynamic obj = JObject.Parse(json);

                var bytes = new List<byte>();
                for (int j = 0; j < payloadSize; j++)
                {
                    bytes.AddRange(Encoding.UTF8.GetBytes("a"));
                }
                obj.payload = Encoding.UTF8.GetString(bytes.ToArray());

                var eventJson = ((JObject)obj).ToString();
                var eventBytes = Encoding.UTF8.GetBytes(eventJson);
                var eventData = new EventData(eventBytes);
                eventData.SetReceivedAt(receivedAt);
                handler.Handle(eventData, "0");
            }
        }
    }
}