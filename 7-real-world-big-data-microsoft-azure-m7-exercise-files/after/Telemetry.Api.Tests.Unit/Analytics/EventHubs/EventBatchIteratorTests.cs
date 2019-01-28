using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Telemetry.Api.Analytics.EventHubs;

namespace Telemetry.Api.Tests.Unit.Analytics.EventHubs
{
    [TestClass]
    public class EventBatchIteratorTests
    {
        [TestMethod]
        public void Iterate_EmptyArray_NoBatches()
        {
            var events = new JArray();

            var iterator = new EventBatchIterator(events) as IEnumerator<IEnumerable<EventData>>;
            Assert.IsFalse(iterator.MoveNext());
        }

        [TestMethod]
        public void Iterate_OneEvent_OneBatch()
        {
            var events = new JArray();
            AddEvents(events, 1);

            var enumerable = new EventBatchIterator(events) as IEnumerable<IEnumerable<EventData>>;
            var iterator = enumerable.GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(1, iterator.Current.Count());
            Assert.IsFalse(iterator.MoveNext());
        }

        [TestMethod]
        public void Iterate_SmallArray_OneBatch()
        {
            var events = new JArray();
            AddEvents(events, 100);

            var enumerable = new EventBatchIterator(events) as IEnumerable;
            var iterator = enumerable.GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(100, ((IEnumerable<EventData>)iterator.Current).Count());
            Assert.IsFalse(iterator.MoveNext());
        }

        [TestMethod]
        public void Iterate_BigArray_MultipleBatches()
        {
            var events = new JArray();
            AddEvents(events, 800);

            var iterator = new EventBatchIterator(events);
            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(560, iterator.Current.Count());

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(240, iterator.Current.Count());

            Assert.IsFalse(iterator.MoveNext());
        }

        [TestMethod]
        public void Iterate_DevicePayload_MultipleBatches()
        {
            var path = Path.Combine(Environment.CurrentDirectory, @"Resources", "device-events-large.json");
            var json = File.ReadAllText(path);
            dynamic obj = JObject.Parse(json);
            var events = (JArray)obj.events;

            var iterator = new EventBatchIterator(events);

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(529, iterator.Current.Count());

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(631, iterator.Current.Count());

            Assert.IsTrue(iterator.MoveNext());
            Assert.IsInstanceOfType(iterator.Current, typeof(IEnumerable<EventData>));
            Assert.AreEqual(2, iterator.Current.Count());

            Assert.IsFalse(iterator.MoveNext());
        }

        [TestMethod]
        public void Iterate_BigArray()
        {
            var events = new JArray();
            AddEvents(events, 800);

            var iterator = new EventBatchIterator(events);
            var batches = 0;
            foreach (var batch in iterator)
            {
                Assert.IsTrue(batch.Count() > 0);
                batches++;
            }

            Assert.AreEqual(2, batches);

            iterator.Reset();
            Assert.IsTrue(iterator.MoveNext());
        }

        private static void AddEvents(JArray events, int eventCount, int payloadSize = 220)
        {
            for (int i = 0; i < eventCount; i++)
            {
                var obj = new
                {
                    deviceId = "123",
                    eventName = "test.event"
                };
                var json = JsonConvert.SerializeObject(obj);
                dynamic evt = JObject.Parse(json);
                var bytes = new List<byte>();
                for (int j = 0; j < payloadSize; j++)
                {
                    bytes.AddRange(Encoding.UTF8.GetBytes("a"));
                }
                evt.payload = Encoding.UTF8.GetString(bytes.ToArray());
                events.Add(evt);
            }
        }
    }
}
