
namespace Telemetry.Entities
{
    public class EventsDbContextFactory
    {
        private readonly string _connectionString;

        public EventsDbContextFactory() { }

        public EventsDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual EventsDbContext GetContext()
        {
            return new EventsDbContext(_connectionString);
        }
    }
}
