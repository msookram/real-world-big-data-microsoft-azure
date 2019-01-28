using Microsoft.SCP;
using Microsoft.SCP.Topology;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    [Active(true)]
    public class Program : TopologyDescriptor
    {
        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static void Main(string[] args)
        {
        }

        public ITopologyBuilder GetTopologyBuilder()
        {
            var topologyBuilder = new TopologyBuilder("TelemetryEventProcessorRealTimeStorm");

            var partitionCount = 16;
            var spoutConfig = GetSpoutConfig(partitionCount);

            topologyBuilder.SetEventHubSpout("EventHubSpout", spoutConfig, partitionCount);

            var javaSerializerInfo = new List<string>() { "microsoft.scp.storm.multilang.CustomizedInteropJSONSerializer" };

            var taskConfig = new StormConfig();
            taskConfig.Set("topology.tick.tuple.freq.secs", "10");

            topologyBuilder.SetBolt("DeviceEventBolt",
                                    DeviceEventBolt.Get,
                                    new Dictionary<string, List<string>>()
                                    {
                                        { Constants.DEFAULT_STREAM_ID, new List<string>(){"eventName", "period"}},
                                        { DeviceEventBolt.DEVICE_LOG_STREAM_ID, new List<string>(){"eventJson"}}
                                    },
                                    partitionCount,
                                    true)
                           .DeclareCustomizedJavaSerializer(javaSerializerInfo)
                           .shuffleGrouping("EventHubSpout")
                           .addConfigurations(taskConfig);            

            topologyBuilder.SetBolt("EventMetricBolt",
                                    EventMetricBolt.Get,
                                    new Dictionary<string, List<string>>(),
                                    4)
                           .fieldsGrouping("DeviceEventBolt", Constants.DEFAULT_STREAM_ID, new List<int>() { 0 })
                           .addConfigurations(taskConfig);

            topologyBuilder.SetBolt("DeviceErrorBolt",
                                    DeviceErrorBolt.Get,
                                    new Dictionary<string, List<string>>(),
                                    partitionCount)
                           .shuffleGrouping("DeviceEventBolt", DeviceEventBolt.DEVICE_LOG_STREAM_ID);

            //global config:
            var topologyConfig = new StormConfig();
            topologyConfig.setMaxSpoutPending(2000);
            topologyConfig.setNumWorkers(partitionCount);
            topologyBuilder.SetTopologyConfig(topologyConfig);

            return topologyBuilder;
        }

        private EventHubSpoutConfig GetSpoutConfig(int partitionCount)
        {
            return new EventHubSpoutConfig("rt-listener", "bR/qu18gOHr3RERYD7zXq5lJyU2rqwOV7P8LnV/oBOs=", "devicetelemetry-prd", "device-events", partitionCount);
        }
    }
}

