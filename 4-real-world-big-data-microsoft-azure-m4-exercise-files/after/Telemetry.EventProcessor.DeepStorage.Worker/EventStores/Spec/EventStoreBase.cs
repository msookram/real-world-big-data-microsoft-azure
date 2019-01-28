using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telemetry.Core;

namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
    public abstract class EventStoreBase : IEventStore
    {
        public string ReceivedAtHour { get; private set; }

        public string PartitionId { get; private set; }

        public IEventStore NextStore { get; private set; }

        protected List<Task> _startedTasks = new List<Task>();

        public abstract int Level { get; }

        private Stopwatch _flushStopwatch;

        public int MaxBufferSize
        {
            get { return Config.Parse<int>("EventStores." + GetType().Name + ".MaxBufferSize"); }
        }

        public TimeSpan? MaxBufferTime
        {
            get { return Config.Parse<TimeSpan>("EventStores.OverdueFlushPeriod"); }
        }

        public abstract void Write(byte[] data);

        public abstract void Flush();

        public virtual void AfterFlush()
        {
            _flushStopwatch = Stopwatch.StartNew();
        }

        protected void AfterWrite()
        {
            if (IsFlushOverdue())
            {
                Flush();
            }
        }

        public bool IsFlushOverdue()
        {
            return MaxBufferTime.HasValue &&
                    _flushStopwatch != null &&
                    _flushStopwatch.ElapsedMilliseconds > MaxBufferTime.Value.TotalMilliseconds;
        }

        public virtual void Initialise(string partitionId, string receivedAtHour)
        {
            PartitionId = partitionId;
            ReceivedAtHour = receivedAtHour;
            if (MaxBufferTime.HasValue)
            {
                _flushStopwatch = Stopwatch.StartNew();
            }
            var nextLevel = (Level + 1).ToString();
            try
            {
                NextStore = Container.Instance.Resolve<IEventStore>(nextLevel);
                if (NextStore != null)
                {
                    NextStore.Initialise(partitionId, receivedAtHour);
                }
            }
            catch { }
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
                Flush();

                if (_startedTasks.Any())
                {
                    Task.WaitAll(_startedTasks.ToArray());
                }
            }
            if (NextStore != null)
            {
                NextStore.Dispose();
            }
        }
    }
}
