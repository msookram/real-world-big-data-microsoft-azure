using Microsoft.SCP;
using System;
using System.Collections.Generic;
using System.Configuration;
using Telemetry.EventProcessor.RealTime.Storm.Events;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    public class EventMetricBolt : ISCPBolt
    {
        private Dictionary<string, long> _eventCounts = new Dictionary<string, long>();
        private Context ctx;

        public EventMetricBolt(Context ctx)
        {
            this.ctx = ctx;

            var inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(string) });
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
        }

        public static EventMetricBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new EventMetricBolt(ctx);
        }

        public void Execute(SCPTuple tuple)
        {
            if (tuple.GetSourceStreamId().Equals(Constants.SYSTEM_TICK_STREAM_ID))
            {
                UpdateDatabase();
            }
            else
            {
                IncrementCount(tuple);
            }
        }

        private void UpdateDatabase()
        {
            using (var db = new EventsDb(ConfigurationManager.AppSettings["EventsDb.ConnectionString"]))
            {
                foreach (var key in _eventCounts.Keys)
                {
                    var keyParts = key.Split('|');
                    var eventName = keyParts[0];
                    var period = keyParts[1];
                    var count = _eventCounts[key];

                    db.MergeEventMetric(eventName, period, count);
                }
                _eventCounts.Clear();
            }
        }

        private void IncrementCount(SCPTuple tuple)
        {
            var eventName = tuple.GetString(0);
            var period = tuple.GetString(1);
            var key = string.Format("{0}|{1}", eventName, period);
            long eventCount = 0;
            if (_eventCounts.ContainsKey(key))
            {
                eventCount = _eventCounts[key];
            }
            eventCount++;
            _eventCounts[key] = eventCount;
        }
    }
}