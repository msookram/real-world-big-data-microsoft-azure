﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Core.EventProcessor
{
    public class EventHandlerFactory
    {
        public IEnumerable<IEventHandler> Handlers {get; private set;}

        public EventHandlerFactory(IEventHandler[] handlers)
        {
            Handlers = handlers;
        }

        public IEnumerable<IEventHandler> GetHandlers(string eventName) 
        {
            var handlers = new List<IEventHandler>();
            foreach (var handler in Handlers)
            {
                if (handler.IsHandled(eventName))
                {
                    handlers.Add(handler);
                }
            }
            return handlers;
        }
    }
}