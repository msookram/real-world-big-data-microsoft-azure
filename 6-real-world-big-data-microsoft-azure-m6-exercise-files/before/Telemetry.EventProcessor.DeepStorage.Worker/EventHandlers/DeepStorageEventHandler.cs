using Microsoft.Practices.Unity;
using Microsoft.ServiceBus.Messaging;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Telemetry.Core;
using Telemetry.Core.EventProcessor;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;
using timers = System.Timers;

namespace Telemetry.EventProcessor.DeepStorage.Worker.EventHandlers
{
    public class DeepStorageEventHandler : IEventHandler
    {
        private readonly Logger _log;

        private static ConcurrentDictionary<string, IEventStore> _EventStores = new ConcurrentDictionary<string, IEventStore>();
        private static timers.Timer _StoreFlushTimer;

        static DeepStorageEventHandler()
        {
            var overdueStoreFlushTime = Config.Parse<TimeSpan>("EventStores.OverdueFlushPeriod");
            _StoreFlushTimer = new timers.Timer(overdueStoreFlushTime.TotalMilliseconds);
            _StoreFlushTimer.Elapsed += FlushOverdueStores;
            _StoreFlushTimer.Start();
        }

        public DeepStorageEventHandler()
        {
            _log = this.GetLogger();
        }

        public bool IsHandled(string eventName)
        {
            //handle all events:
            return true;
        }

        public void Handle(EventData eventData, string partitionId)
        {
            var store = GetEventStore(eventData, partitionId);
            var bytes = eventData.GetBytes();
            store.Write(bytes);
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
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FlushOverdueStores();
            }
        }
    }
}
