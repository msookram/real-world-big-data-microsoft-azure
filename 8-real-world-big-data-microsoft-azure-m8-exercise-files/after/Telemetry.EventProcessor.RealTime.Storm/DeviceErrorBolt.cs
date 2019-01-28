using Microsoft.SCP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using Telemetry.EventProcessor.RealTime.Storm.Events;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    public class DeviceErrorBolt: ISCPBolt
    {
        private Dictionary<string, int> _deviceEvents = new Dictionary<string, int>();
        private Context ctx;

        public DeviceErrorBolt(Context ctx)
        {
            this.ctx = ctx;

            var inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(DeviceEventBolt.DEVICE_LOG_STREAM_ID, new List<Type>() { typeof(string)});

            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
        }

        public static DeviceErrorBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new DeviceErrorBolt(ctx);
        }

        public void Execute(SCPTuple tuple)
        {
            Context.Logger.Info(" --EventSummaryBolt -> Execute");

            var eventJson = (string)tuple.GetString(0);
            var logEvent = JsonConvert.DeserializeObject<DeviceLogEvent>(eventJson, Program.JsonSettings);

            if (logEvent.Severity == DeviceLogEvent.SeverityType.Error)
            {
                var receivedDateTime = logEvent.ReceivedAt > long.MinValue ? Program.Epoch.AddMilliseconds(logEvent.ReceivedAt) : DateTime.UtcNow;
                var period = receivedDateTime.ToString("yyyyMMddHH");

                var appSettings = ConfigurationManager.AppSettings;
                var client = new EventsHBase(appSettings["EventsHBase.ClusterUrl"], appSettings["EventsHBase.Username"] , appSettings["EventsHBase.Password"]);

                client.IncrementDeviceErrorCount(logEvent.OtaVersion, period, logEvent.DeviceId);

                Context.Logger.Info(" --EventSummaryBolt -> updated hbase");
            }
        }
    }
}
