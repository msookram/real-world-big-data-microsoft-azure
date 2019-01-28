﻿using ICSharpCode.SharpZipLib.GZip;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
    public class DiskEventStore : EventStoreBase
    {
        private string _filePath;
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public override int Level
        {
            get { return 2; }
        }

        public override void Initialise(string partitionId, string receivedAtHour)
        {
            base.Initialise(partitionId, receivedAtHour);
            var fileName = string.Format("{0}p{1}-{2}.json.gz", receivedAtHour, partitionId, Guid.NewGuid().ToString().Substring(0, 4));
            var rootPath = Path.GetTempPath();
            if (RoleEnvironment.IsAvailable)
            {
                var localResource = RoleEnvironment.GetLocalResource("DiskEventStore");
                rootPath = localResource.RootPath;
            }
            _filePath = Path.Combine(rootPath, fileName);
            using (File.Create(_filePath)) { }
        }

        public override void Write(byte[] data)
        {
            var compressedStream = new MemoryStream();

            try
            {
                using (var inputStream = new MemoryStream(data))
                {
                    using (var compressionStream = new GZipOutputStream(compressedStream))
                    {
                        compressionStream.SetLevel(9);
                        inputStream.CopyTo(compressionStream);
                        compressionStream.Flush();
                    }
                }

                var compressedData = compressedStream.ToArray();
                _lock.Wait();

                using (var outputStream = File.OpenWrite(_filePath))
                {
                    outputStream.Position = outputStream.Length;
                    outputStream.Write(compressedData, 0, compressedData.Length);
                    outputStream.Flush();
                }
            }
            finally
            {
                compressedStream.Dispose();
                _lock.Release();
            }

            var info = new FileInfo(_filePath);
            var bufferSize = info.Length;
            if (bufferSize > MaxBufferSize)
            {
                Flush();
            }
        }

        public override void Flush()
        {
            var data = new byte[0];

            try
            {
                _lock.Wait();
                data = File.ReadAllBytes(_filePath);
                File.Delete(_filePath);
                using (File.Create(_filePath)) { }
            }
            finally
            {
                _lock.Release();
            }

            if (data.Length > 0)
            {
                _startedTasks.Add(Task.Factory.StartNew(() => NextStore.Write(data)));
            }
        }
    }
}