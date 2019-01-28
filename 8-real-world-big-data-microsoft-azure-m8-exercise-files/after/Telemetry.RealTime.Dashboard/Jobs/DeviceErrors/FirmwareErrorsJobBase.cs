using dashing.net.streaming;
using Microsoft.HBase.Client;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using System;
using System.Configuration;
using System.Linq;
using System.Text;

namespace dashing.net.Jobs.DeviceErrors
{
    public abstract class FirmwareErrorsJobBase
    {
        protected abstract int FirmwareVersion { get; }

        private bool _isRunning;

        protected void SendFirmwareErrorMessages(object state)
        {
            if (!_isRunning)
            {
                try
                {
                    _isRunning = true;
                    SendFirmwareErrorMessages();
                }
                finally
                {
                    _isRunning = false;
                }
            }
        }

        public void SendFirmwareErrorMessages()
        {
            var deviceCount = 0;
            var errorCount = 0L;
            var averageErrorsPerDevice = 0;

            GetFirmwareVersionErrors(out deviceCount, out errorCount, out averageErrorsPerDevice);

            var averageMessage = new
            {
                id = "average-errors-v" + FirmwareVersion + ".0",
                current = averageErrorsPerDevice
            };
            Dashing.SendMessage(averageMessage);

            var errorCountMessage = new
            {
                id = "total-errors-v" + FirmwareVersion + ".0",
                current = errorCount
            };
            Dashing.SendMessage(errorCountMessage);

            var deviceCountMessage = new
            {
                id = "total-devices-v" + FirmwareVersion + ".0",
                current = deviceCount
            };
            Dashing.SendMessage(deviceCountMessage);
        }

        protected void GetFirmwareVersionErrors(out int deviceCount, out long errorCount, out int averageErrorsPerDevice)
        {
            var tableName = "device-errors";
            var rowKeyFormat = "v{0}.0|20150608{1}";

            var currentHour = DateTime.UtcNow.Hour;       
            
            var startRowKey = string.Format(rowKeyFormat, FirmwareVersion, "00");
            var endRowKey = string.Format(rowKeyFormat, FirmwareVersion, (currentHour + 1).ToString("D2"));

            var scanSettings = new Scanner()
            {
                batch = 5000,
                startRow = Encoding.UTF8.GetBytes(startRowKey),
                endRow = Encoding.UTF8.GetBytes(endRowKey)
            };

            var client = GetHBaseClient();

            var scannerInfo = client.CreateScanner(tableName, scanSettings);
            CellSet next = null;

            var devices = 0D;
            var errors = 0D;
            
            while ((next = client.ScannerGetNext(scannerInfo)) != null)
            {
                foreach (var row in next.rows)
                {
                    devices += row.values.LongCount();
                    var values = row.values.Select(x=> BitConverter.ToInt32(x.data, 0));
                    errors += values.Sum();
                }
            }

            deviceCount = (int)devices;
            errorCount = (long)errors;
            averageErrorsPerDevice = (int)Math.Round(errors / devices);
        }

        private static HBaseClient GetHBaseClient()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var creds = new ClusterCredentials(new Uri(appSettings["EventsHBase.ClusterUrl"]), appSettings["EventsHBase.Username"], appSettings["EventsHBase.Password"]);
            var client = new HBaseClient(creds);
            return client;
        }
    }
}