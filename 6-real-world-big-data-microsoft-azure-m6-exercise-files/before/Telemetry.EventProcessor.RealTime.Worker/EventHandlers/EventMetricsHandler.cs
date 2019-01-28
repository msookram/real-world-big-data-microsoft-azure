using Microsoft.ServiceBus.Messaging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telemetry.Core.EventProcessor;
using Telemetry.Entities;
using Telemetry.Entities.Model;
using Telemetry.RealTime.Worker.Buffers.ModelBuffer;

namespace Telemetry.RealTime.Worker.EventHandlers
{
    public class EventMetricsHandler : IEventHandler
    {
        private readonly Logger _log;
        private readonly EventsDbContextFactory _dbFactory;
        private readonly ModelBuffer<EventMetric> _buffer;

        public EventMetricsHandler(EventsDbContextFactory dbFactory)
        {
            _log = this.GetLogger();
            _dbFactory = dbFactory;
            _buffer = new ModelBuffer<EventMetric>(
                x => SaveMetrics(x),
                y => string.Format("{0}p{1}_{2}", y.Period, y.PartitionId, y.EventName));
        }

        private void SaveMetrics(IEnumerable<EventMetric> metrics)
        {
            using (var db = _dbFactory.GetContext())
            {
                foreach (var metric in metrics)
                {
                    var existingMetric = db.EventMetrics.FirstOrDefault(
                        x => x.PartitionId == metric.Period &&
                        x.PartitionId == metric.PartitionId &&
                        x.EventName == metric.EventName);

                    if (existingMetric == null)
                    {
                        db.EventMetrics.Add(metric);
                    }
                    else
                    {
                        existingMetric.Count += metric.Count;
                        existingMetric.ProcessedAt = metric.ProcessedAt;
                    }
                }
                db.SaveChanges();
            }
        }

        public bool IsHandled(string eventName)
        {
            return true;
        }

        public void Handle(EventData eventData, string partitionId)
        {
            var eventName = eventData.GetEventName();
            var period = eventData.GetReceivedAtHour();

            EventMetric metric = GetMetric(eventName, period, partitionId);
            metric.Count += 1;
            metric.ProcessedAt = DateTime.UtcNow;
        }

        private EventMetric GetMetric(string eventName, string period, string partitionId)
        {
            var newMetric = new EventMetric
            {
                Period = period,
                PartitionId = partitionId,
                EventName = eventName,
                Count = 0
            };
            var existingMetric = _buffer.Get(newMetric);
            if (existingMetric != null)
            {
                return existingMetric;
            }
            else
            {
                _buffer.Add(newMetric);
                return newMetric;
            }            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer.Dispose();
            }
        }
    }
}
