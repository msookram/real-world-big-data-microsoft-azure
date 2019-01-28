using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.EventProcessor.RealTime.Storm.Events
{
    [Serializable]
    public class DeviceLogEvent : DeviceEvent
    {
        public string Severity { get; set; }

        public string OtaVersion { get; set; }

        public struct SeverityType
        {
            public const string Error = "E";

            public const string Warn = "W";

            public const string Info = "I";
        }
    }
}
