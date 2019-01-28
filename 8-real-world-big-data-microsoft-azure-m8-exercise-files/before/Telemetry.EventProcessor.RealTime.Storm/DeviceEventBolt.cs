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
        private Context ctx;

        private static JsonSerializerSettings _JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public DeviceEventBolt(Context ctx)
        {
            this.ctx = ctx;

            var inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            var outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(string) });

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
                var deviceEvent = JsonConvert.DeserializeObject<DeviceEvent>(eventJson, _JsonSettings);
                var receivedDateTime = deviceEvent.ReceivedAt > long.MinValue ? Epoch.AddMilliseconds(deviceEvent.ReceivedAt) : DateTime.UtcNow;

                ctx.Emit(new Values(deviceEvent.EventName, receivedDateTime.ToString("yyyyMMddHH")));
                ctx.Ack(tuple);
            }
        }
    }
}