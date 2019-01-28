using Microsoft.HBase.Client;
using Microsoft.HBase.Client.Filters;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System;
using System.Text;

namespace Telemetry.EventProcessor.RealTime.Storm
{
    public class EventsHBase
    {
        private const string TABLE_NAME = "device-errors";

        private HBaseClient _client;

        public EventsHBase(string clusterUrl, string userName, string password)
        {
            var creds = new ClusterCredentials(new Uri(clusterUrl), userName, password);
            _client = new HBaseClient(creds);
        }
        
        public void IncrementDeviceErrorCount(string firmwareVersion, string period, string deviceId)
        {
            var rowKey = string.Format("{0}|{1}", firmwareVersion, period);
            var rowKeyBytes = Encoding.UTF8.GetBytes(rowKey);

            //scan to find a match for the device:
            var columnNameBytes = Encoding.UTF8.GetBytes(deviceId);
            var filter = new QualifierFilter(CompareFilter.CompareOp.Equal, new BinaryComparator(columnNameBytes));
            var scanner = new Scanner()
            {
                batch = 1, // maximum one cell per period, firmware version & device
                startRow = rowKeyBytes,
                endRow = rowKeyBytes,
                filter = filter.ToEncodedString()
            };

            var scannerInfo = _client.CreateScanner(TABLE_NAME, scanner);
            var existingCells = _client.ScannerGetNext(scannerInfo);
            if (existingCells != null)
            {
                var currentErrorCount = BitConverter.ToInt32(existingCells.rows[0].values[0].data, 0);
                currentErrorCount++;
                existingCells.rows[0].values[0].data = BitConverter.GetBytes(currentErrorCount);
                _client.StoreCells(TABLE_NAME, existingCells);
            }
            else
            {
                var set = new CellSet();
                var row = new CellSet.Row { key = rowKeyBytes };
                set.rows.Add(row);

                var columnName = "d:" + deviceId;
                var value = new Cell
                {
                    column = Encoding.UTF8.GetBytes(columnName),
                    data = BitConverter.GetBytes(1)
                };
                row.values.Add(value);
                _client.StoreCells(TABLE_NAME, set);
            }
        }
    }
}
