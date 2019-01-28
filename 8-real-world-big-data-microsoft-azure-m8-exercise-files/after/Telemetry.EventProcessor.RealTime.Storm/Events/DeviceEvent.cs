using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.EventProcessor.RealTime.Storm.Events
{
    [Serializable]
    public class DeviceEvent
    {
        public string DeviceId { get; set; }

        public string EventName { get; set; }

        public long ReceivedAt { get; set; }

        public string ReceivedAtHour { get; set; }
    }
}
