using System;

namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
    public interface IEventStore : IDisposable
    {
        int Level { get; }

        string ReceivedAtHour { get; }

        string PartitionId { get; }
        
        void Initialise(string partitionId, string receivedAtHour);

        void Write(byte[] value);

        void Flush();

        bool IsFlushOverdue();
    }
}
