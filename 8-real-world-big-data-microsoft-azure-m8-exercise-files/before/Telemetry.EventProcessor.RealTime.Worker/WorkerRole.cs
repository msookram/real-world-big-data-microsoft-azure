using Microsoft.Practices.Unity;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using NLog;
using System;
using System.Threading;
using Telemetry.Core;
using Telemetry.Core.EventProcessor;
using Telemetry.Core.EventProcessor.Processors;
using Telemetry.Entities;
using Telemetry.RealTime.Worker.EventHandlers;

namespace Telemetry.RealTime.Worker
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

        public override bool OnStart()
        {
            var container = Container.Instance;

            var dbConnectionString = Config.Get("EventsDb.ConnectionString");
            container.RegisterType<EventsDbContextFactory>(new InjectionConstructor(dbConnectionString));

            container.RegisterType<IEventHandler, EventMetricsHandler>("EventMetricsHandler");
            container.RegisterType<EventHandlerFactory>();

            container.RegisterType<IEventProcessor, HandledEventProcessor>();
            container.RegisterType<IEventProcessorFactory, EventProcessorFactory>();

            var eventHubName = Config.Get("RealTime.EventHubName");
            var consumerGroupName = Config.Get("RealTime.ConsumerGroupName");
            var eventHubConnectionString = Config.Get("RealTime.InputConnectionString");
            var checkpointConnectionString = Config.Get("RealTime.CheckpointConnectionString");

            container.RegisterType<EventReceiver>(new InjectionConstructor(
                new ResolvedParameter<IEventProcessorFactory>(),
                eventHubName, consumerGroupName,
                eventHubConnectionString, checkpointConnectionString));

            _receiver = container.Resolve<EventReceiver>();

            return base.OnStart();
        }

        public override void Run()
        {
            _receiver.RegisterProcessorAsync().Wait();

            CompletedEvent.WaitOne();
        }

        public override void OnStop()
        {
            _receiver.UnregisterProcessorAsync().Wait();

            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
