using Microsoft.ServiceBus.Messaging;
using System;

namespace Telemetry.Core.EventProcessor
{
    public interface IEventHandler : IDisposable
    {
        bool IsHandled(string eventName);

        void Handle(EventData eventData, string partitionId);     
    }
}
