using System.Data.Entity;
using Telemetry.Entities.Model;

namespace Telemetry.Entities
{
    public partial class EventsDbContext : DbContext
    {
        public EventsDbContext() { }

        public EventsDbContext(string connectionString) : base(connectionString) { }

        public virtual DbSet<EventMetric> EventMetrics { get; set; }
    }
}
