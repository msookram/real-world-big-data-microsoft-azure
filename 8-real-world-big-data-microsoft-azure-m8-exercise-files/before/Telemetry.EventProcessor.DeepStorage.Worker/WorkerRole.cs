using Microsoft.Practices.Unity;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using NLog;
using System;
using System.Threading;
using Telemetry.Core;
using Telemetry.Core.EventProcessor;
using Telemetry.Core.EventProcessor.Processors;
using Telemetry.EventProcessor.DeepStorage.Worker.EventHandlers;
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
            var container = Container.Instance;

            //event stores:
            container.RegisterType<IEventStore, MemoryEventStore>("1");
            container.RegisterType<IEventStore, DiskEventStore>("2");
            container.RegisterType<IEventStore, BlobStorageEventStore>("3");

            //event handlers:
            container.RegisterType<IEventHandler, DeepStorageEventHandler>("DeepStorageEventHandler");
            container.RegisterType<EventHandlerFactory>();

            //event processors:
            container.RegisterType<IEventProcessor, HandledEventProcessor>();
            container.RegisterType<IEventProcessorFactory, EventProcessorFactory>();

            //event receiver:
            var eventHubName = Config.Get("DeepStorage.EventHubName");
            var consumerGroupName = Config.Get("DeepStorage.ConsumerGroupName");
            var eventHubConnectionString = Config.Get("DeepStorage.InputConnectionString");
            var checkpointConnectionString = Config.Get("DeepStorage.CheckpointConnectionString");

            container.RegisterType<EventReceiver>(new InjectionConstructor(
                                                    new ResolvedParameter<IEventProcessorFactory>(),
                                                    eventHubName,
                                                    consumerGroupName,
                                                    eventHubConnectionString,
                                                    checkpointConnectionString));

            _receiver = container.Resolve<EventReceiver>();

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
