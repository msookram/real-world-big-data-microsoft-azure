using Microsoft.ServiceBus.Messaging;

namespace Telemetry.Core.EventProcessor.Processors
{
    public class EventProcessorFactory : IEventProcessorFactory
    {
        private readonly IEventProcessor _processor;

        public EventProcessorFactory(IEventProcessor processor)
        {
            _processor = processor;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return _processor;
        }
    }
}
