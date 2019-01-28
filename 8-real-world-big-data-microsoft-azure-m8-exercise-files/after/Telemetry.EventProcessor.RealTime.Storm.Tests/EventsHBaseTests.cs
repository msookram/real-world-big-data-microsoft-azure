using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Microsoft.HBase.Client;
using System.Text;
using Microsoft.HBase.Client.Filters;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System.Linq;

namespace Telemetry.EventProcessor.RealTime.Storm.Tests
{
    //[Ignore] //manual integration test
    [TestClass]
    public class EventsHBaseTests
    {
        [TestMethod]
        public void IncrementDeviceErrorCount()
        {
            var db = new EventsHBase("https://telemetryprdhbase.azurehdinsight.net", "admin", "TescoApr!5");

            var deviceId = Guid.NewGuid().ToString();
            var period = "2015060409";
            var firmwareVersion = Guid.NewGuid().ToString().Substring(0,4);

            for (int i = 0; i < 10; i++ )
            {
                db.IncrementDeviceErrorCount(firmwareVersion, period, deviceId);
                AssertRowExists(firmwareVersion, period, 1);
                AssertErrorCount(firmwareVersion, period, period, (i+1), deviceId);
            }           

            var period2 = "2015060410";
            db.IncrementDeviceErrorCount(firmwareVersion, period2, deviceId);
            AssertRowExists(firmwareVersion, period2, 1);
            AssertErrorCount(firmwareVersion, period2, period2, 1, deviceId);

            //scans for multipe rows are not inclusive for the end row, so we need to increment:
            var period3 = "2015060411";
            AssertErrorCount(firmwareVersion, period, period3, 11);
            AssertErrorCount(firmwareVersion, period, period3, 11, deviceId);
        }

        private void AssertRowExists(string firmwareVersion, string period, int expectedColumnCount)
        {
            var creds = new ClusterCredentials(new Uri("https://telemetryprdhbase.azurehdinsight.net"), "admin", "TescoApr!5");
            var client = new HBaseClient(creds);

            var tableName = "device-errors";
            var rowKeyFormat = "{0}|{1}";

            var rowKey = string.Format(rowKeyFormat, firmwareVersion, period);

            var cells = client.GetCells(tableName, rowKey);
            Assert.IsNotNull(cells);
            Assert.AreEqual(1, cells.rows.Count);

            Assert.AreEqual(expectedColumnCount, cells.rows[0].values.Count);
        }

        private void AssertErrorCount(string firmwareVersion, string startPeriod, string endPeriod, int expectedCount, string deviceId = null)
        {
            var creds = new ClusterCredentials(new Uri("https://telemetryprdhbase.azurehdinsight.net"), "admin", "TescoApr!5");
            var client = new HBaseClient(creds);

            var tableName = "device-errors";
            var rowKeyFormat = "{0}|{1}";

            var startRowKey = string.Format(rowKeyFormat, firmwareVersion, startPeriod);
            var endRowKey = string.Format(rowKeyFormat, firmwareVersion, endPeriod);                    

            var scanSettings = new Scanner()
            {
                batch = 10, 
                startRow = Encoding.UTF8.GetBytes(startRowKey),
                endRow = Encoding.UTF8.GetBytes(endRowKey)               
            };

            if (!string.IsNullOrEmpty(deviceId))
            {
                var columnNameBytes = Encoding.UTF8.GetBytes(deviceId);   
                var filter = new QualifierFilter(CompareFilter.CompareOp.Equal, new BinaryComparator(columnNameBytes));
                scanSettings.filter = filter.ToEncodedString();
            }

            var scannerInfo = client.CreateScanner(tableName, scanSettings);
            CellSet next = null;

            var errorCount = 0; 

            while ((next = client.ScannerGetNext(scannerInfo)) != null)
            {                
                foreach (var row in next.rows)
                {
                    errorCount += row.values.Sum(x => BitConverter.ToInt32(x.data, 0));
                }
            }

            Assert.AreEqual(expectedCount, errorCount);
        }
    }
}
