using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.RealTime.Worker.Tests.Unit.Stubs.Models
{
    public class StubModel
    {
        public string Period { get; set; }

        public string PartitionId { get; set; }

        public int Index { get; set; }

        public int Count { get; set; }
    }
}
