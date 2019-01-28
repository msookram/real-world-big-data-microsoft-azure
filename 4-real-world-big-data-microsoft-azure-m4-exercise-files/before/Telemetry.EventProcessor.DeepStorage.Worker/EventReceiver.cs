using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telemetry.Core;
using Telemetry.EventProcessor.DeepStorage.Worker.EventProcessors;

namespace Telemetry.EventProcessor.DeepStorage.Worker
{
    public class EventReceiver
    {
        private readonly EventProcessorHost _host;

        public EventReceiver()
        {
            _host = new EventProcessorHost(Environment.MachineName, Config.Get("DeepStorage.EventHubName"),
                Config.Get("DeepStorage.ConsumerGroupName"), Config.Get("DeepStorage.InputConnectionString"),
                Config.Get("DeepStorage.CheckpointConnectionString"));
        }

        public async Task RegisterProcessorAsync()
        {
            var processorOptions = new EventProcessorOptions
            {
                MaxBatchSize = 5000,
                PrefetchCount = 1000
            };
            await _host.RegisterEventProcessorAsync<DeepStorageEventProcessor>();
        }

        public async Task UnregisterProcessorAsync()
        {
            if (_host != null)
            {
                await _host.UnregisterEventProcessorAsync();
            }
        }
    }
}
