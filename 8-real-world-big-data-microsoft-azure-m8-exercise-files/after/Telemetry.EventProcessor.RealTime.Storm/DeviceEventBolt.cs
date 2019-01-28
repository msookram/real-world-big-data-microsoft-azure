using Microsoft.SCP;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Telemetry.EventProcessor.RealTime.Storm.Events;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    public class DeviceEventBolt : ISCPBolt
    {
        public const string DEVICE_LOG_STREAM_ID = "_deviceLogs";

        private Context ctx;

        public DeviceEventBolt(Context ctx)
        {
            this.ctx = ctx;

            var inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            var outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(string) });
            outputSchema.Add(DEVICE_LOG_STREAM_ID, new List<Type>() { typeof(string) });

            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));
            this.ctx.DeclareCustomizedDeserializer(new CustomizedInteropJSONDeserializer());
        }

        public static DeviceEventBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new DeviceEventBolt(ctx);
        }

        public void Execute(SCPTuple tuple)
        {
            if (!tuple.GetSourceStreamId().Equals(Constants.SYSTEM_TICK_STREAM_ID))
            {
                var eventJson = (string)tuple.GetString(0);
                var deviceEvent = JsonConvert.DeserializeObject<DeviceEvent>(eventJson, Program.JsonSettings);
                var receivedDateTime = deviceEvent.ReceivedAt > long.MinValue ? Program.Epoch.AddMilliseconds(deviceEvent.ReceivedAt) : DateTime.UtcNow;

                ctx.Emit(Constants.DEFAULT_STREAM_ID, new Values(deviceEvent.EventName, receivedDateTime.ToString("yyyyMMddHH")));
                ctx.Ack(tuple);                
              
                if (deviceEvent.EventName == "device.logs.message")
                {
                    ctx.Emit(DEVICE_LOG_STREAM_ID, new Values(eventJson));
                }
            }
        }
    }
}