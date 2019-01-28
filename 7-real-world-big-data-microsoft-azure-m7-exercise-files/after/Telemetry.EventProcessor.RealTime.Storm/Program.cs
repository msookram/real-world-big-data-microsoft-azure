using Microsoft.SCP;
using Microsoft.SCP.Topology;
using System.Collections.Generic;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    [Active(true)]
    class Program : TopologyDescriptor
    {
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
                                        { Constants.DEFAULT_STREAM_ID, new List<string>(){"eventName", "period"}}
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
                           .fieldsGrouping("DeviceEventBolt", new List<int>() { 0 })
                           .addConfigurations(taskConfig);

            return topologyBuilder;
        }

        private EventHubSpoutConfig GetSpoutConfig(int partitionCount)
        {
            return new EventHubSpoutConfig("{YOUR-ACCESS-KEY-NAME}", "{YOUR-ACCESS-KEY}", "{YOUR-NAMESPACE}", "device-events", partitionCount);
        }
    }
}

