using System;
using System.Data;
using System.Data.SqlClient;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    public class EventsDb : IDisposable
    {
        private SqlConnection _connection;

        public EventsDb(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public void MergeEventMetric(string eventName, string period, long eventCount)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "UpsertEventMetric";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@EventName", SqlDbType.NVarChar, 40).Value = eventName;
                command.Parameters.Add("@Period", SqlDbType.NVarChar, 10).Value = period;
                command.Parameters.Add("@PartitionId", SqlDbType.NVarChar, 5).Value = "."; //not used by Storm
                command.Parameters.Add("@Count", SqlDbType.BigInt).Value = eventCount;

                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && _connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
