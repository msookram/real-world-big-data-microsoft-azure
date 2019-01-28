using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.ServiceRuntime;
using NLog;
using System;
using System.Threading;
using Telemetry.Core;
using Telemetry.Core.Logging;
using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;

namespace Telemetry.EventProcessor.DeepStorage.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly Logger _log;
        private EventReceiver _receiver;
        private ManualResetEvent CompletedEvent = new ManualResetEvent(false);      

        public WorkerRole()
        {
            _log = this.GetLogger();
        }

        public override void Run()
        {
            _receiver.RegisterProcessorAsync().Wait();

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            Container.Instance.RegisterType<IEventStore, MemoryEventStore>("1");
            Container.Instance.RegisterType<IEventStore, DiskEventStore>("2");
            Container.Instance.RegisterType<IEventStore, BlobStorageEventStore>("3");

            _receiver = new EventReceiver();

            return base.OnStart();
        }

        public override void OnStop()
        {
            _receiver.UnregisterProcessorAsync().Wait();

            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
