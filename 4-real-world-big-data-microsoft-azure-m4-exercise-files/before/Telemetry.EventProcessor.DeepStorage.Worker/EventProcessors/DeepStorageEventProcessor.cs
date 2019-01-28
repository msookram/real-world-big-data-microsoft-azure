using Microsoft.Practices.Unity;
using Microsoft.ServiceBus.Messaging;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telemetry.Core;
using Telemetry.Core.Logging;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;
using timers = System.Timers;

namespace Telemetry.EventProcessor.DeepStorage.Worker.EventProcessors
{
    public class DeepStorageEventProcessor : IEventProcessor
    {
        private readonly Logger _log;

        private static ConcurrentDictionary<string, IEventStore> _EventStores = new ConcurrentDictionary<string, IEventStore>();
        private static timers.Timer _StoreFlushTimer;

        static DeepStorageEventProcessor()
        {
            var overdueStoreFlushTime = Config.Parse<TimeSpan>("EventStores.OverdueFlushPeriod");
            _StoreFlushTimer = new timers.Timer(overdueStoreFlushTime.TotalMilliseconds);
            _StoreFlushTimer.Elapsed += FlushOverdueStores;
            _StoreFlushTimer.Start();
        }
        public DeepStorageEventProcessor()
        {
            _log = this.GetLogger();
        }

        public async Task OpenAsync(PartitionContext context)
        {
            _log.InfoEvent("Open",
                   new Facet { Name = "eventHubPath", Value = context.EventHubPath },
                   new Facet { Name = "partitionId", Value = context.Lease.PartitionId },
                   new Facet { Name = "offset", Value = context.Lease.Offset });
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            _log.InfoEvent("Close",
                    new Facet { Name = "reason", Value = reason },
                    new Facet { Name = "partitionId", Value = context.Lease.PartitionId },
                    new Facet { Name = "offset", Value = context.Lease.Offset });
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            _log.DebugEvent("HandleEvents",
                    new Facet { Name = "state", Value = "Received" },
                    new Facet { Name = "eventCount", Value = messages.Count() },
                    new Facet { Name = "eventHubPath", Value = context.EventHubPath },
                    new Facet { Name = "partitionId", Value = context.Lease.PartitionId });
       
            foreach (var eventData in messages)
            {
                var store = GetEventStore(eventData, context.Lease.PartitionId);
                var bytes = eventData.GetBytes();
                store.Write(bytes);
            }

            await context.CheckpointAsync();
        }

        private IEventStore GetEventStore(EventData eventData, string partitionId)
        {
            var receivedAt = eventData.GetReceivedAtHour();
            var key = string.Format("{0}p{1}", receivedAt, partitionId);

            if (!_EventStores.ContainsKey(key))
            {
                var store = Container.Instance.Resolve<IEventStore>("1");
                store.Initialise(partitionId, receivedAt);
                _EventStores[key] = store;
            }

            return _EventStores[key];
        }

        private static void FlushOverdueStores(object sender, timers.ElapsedEventArgs e)
        {
            FlushOverdueStores();
        }

        public static void FlushOverdueStores()
        {
            var overdueStoreKeys = new List<string>();
            var beforeCount = _EventStores.Count;

            foreach (var store in _EventStores)
            {
                if (store.Value.IsFlushOverdue())
                {
                    overdueStoreKeys.Add(store.Key);
                }
            }
            foreach (var overdueStoreKey in overdueStoreKeys)
            {
                IEventStore overdueStore;
                if (_EventStores.TryRemove(overdueStoreKey, out overdueStore))
                {
                    overdueStore.Flush();
                    overdueStore.Dispose();
                }
            }

            var afterCount = _EventStores.Count;
        }
    }
}
