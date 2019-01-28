using Microsoft.ServiceBus.Messaging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telemetry.Core.Logging;

namespace Telemetry.Core.EventProcessor
{
    public class HandledEventProcessor : IEventProcessor
    {
        protected Logger _log;        
        private readonly EventHandlerFactory _handlerFactory;

        public HandledEventProcessor(EventHandlerFactory handlerFactory)
        {
            _log = this.GetLogger();
            _handlerFactory = handlerFactory;
        }

        public async Task OpenAsync(PartitionContext context)
        {
            _log.InfoEvent("Open",
                    new Facet { Name = "eventHubPath", Value = context.EventHubPath },
                    new Facet { Name = "partitionId", Value = context.Lease.PartitionId },
                    new Facet { Name = "offset", Value = context.Lease.Offset });
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                foreach (EventData eventData in events)
                {
                    var eventName = eventData.GetEventName();
                    var handlers = _handlerFactory.GetHandlers(eventName);
                    if (handlers.Any())
                    {
                        foreach (var handler in handlers)
                        {
                            SafelyHandleEvent(handler, eventName, eventData, context);
                        }      
                    }
                    else
                    {
                        _log.WarnEvent("NoEventHandlers",
                            new Facet { Name = "eventName", Value = eventName });
                    }
                }
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                _log.ErrorEvent("ProcessEventsFailed", ex,
                    new Facet { Name = "eventCount", Value = events.Count() },
                    new Facet { Name = "eventHubPath", Value = context.EventHubPath },
                    new Facet { Name = "partitionId", Value = context.Lease.PartitionId });
            }
        }

        private void SafelyHandleEvent(IEventHandler handler, string eventName, EventData eventData, PartitionContext context)
        {
            try
            {
                handler.Handle(eventData.Clone(), context.Lease.PartitionId);
            }
            catch (Exception ex)
            {
                _log.ErrorEvent("HandleEventFailed", ex,
                    new Facet { Name = "handlerFullName", Value = handler.GetType().FullName },
                    new Facet { Name = "eventHubPath", Value = context.EventHubPath },
                    new Facet { Name = "partitionId", Value = context.Lease.PartitionId });
            }
        }

        public virtual async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            _log.InfoEvent("Close",
                    new Facet { Name = "reason", Value = reason },
                    new Facet { Name = "partitionId", Value = context.Lease.PartitionId },
                    new Facet { Name = "offset", Value = context.Lease.Offset });

            foreach (var handler in _handlerFactory.Handlers)
            {
                handler.Dispose();
            }
        }
    }
}
