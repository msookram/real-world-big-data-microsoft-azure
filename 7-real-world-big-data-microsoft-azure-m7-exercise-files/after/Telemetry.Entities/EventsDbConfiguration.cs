using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Entities
{
    public class EventsDbConfiguration : DbConfiguration
    {
        public EventsDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () =>
                new SqlAzureExecutionStrategy(10, TimeSpan.FromSeconds(5)));
        }
    }
}
