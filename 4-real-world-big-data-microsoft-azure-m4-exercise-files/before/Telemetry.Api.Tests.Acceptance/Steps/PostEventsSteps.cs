using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TechTalk.SpecFlow;
using Telemetry.Api.Tests.Stubs.EventIngestionApi;
using Telemetry.Api.Tests.Stubs.EventIngestionApi.Controllers;

namespace Telemetry.Api.Tests.Acceptance.Steps
{
    [Binding]
    public class DeviceMonitoringSteps : StepBase
    {
        [Given(@"a device with id '(.*)' has recorded an event payload")]
        public void GivenADeviceWithIdHasRecordedAnEvent(string deviceId)
        {
            DeviceId = deviceId;
        }      

        [When(@"the device reports the event")]
        public void WhenTheDeviceReportsTheEvent()
        {
            EventsController.LastEventBatch = null;
            PostApiRequest("events");
        }

        [Then(@"the payload is sent to the Telemetry API")]
        public void ThenThePayloadIsSentToTheTelemetryAPI()
        {
            Assert.IsNotNull(EventsController.LastEventBatch, "No post in stub - ServerStartException: {0}", SelfHost.ServerStartException);
        }

        [Then(@"the payload is not sent to the Telemetry API")]
        public void ThenThePayloadIsNotSentToTheTelemetryAPI()
        {
            Assert.IsNull(EventsController.LastEventBatch);
        }

        [Then(@"the API relays (.*) event to the event ingestion API")]
        public void ThenAPIRelaysEventsToTheEventIngestionAPI(int expectedEventCount)
        {
            Assert.AreEqual(expectedEventCount, EventsController.LastEventBatch.Count);
        }
        
        [Then(@"the ingestion API records an event with the expected Fields and Values")]
        public void ThenTheIngestionAPIRecordsAnEventWithTheExpectedFieldsAndValues(Table table)
        {
            AssertObjectContainsTheExpectedFieldsAndValues(EventsController.LastEventBatch.Single(), table);
        }      

        [Then(@"the ingestion API records the first event with the expected Fields and Values")]
        public void ThenTheAnaltyicsAPIRecordsTheFirstEventWithTheExpectedFieldsAndValues(Table table)
        {
            AssertObjectContainsTheExpectedFieldsAndValues(EventsController.LastEventBatch.First(), table);
        }

        [Then(@"the ingestion API records the last event with the expected Fields and Values")]
        public void ThenTheAnaltyicsAPIRecordsTheLastEventWithTheExpectedFieldsAndValues(Table table)
        {
            AssertObjectContainsTheExpectedFieldsAndValues(EventsController.LastEventBatch.Last(), table);
        }
    }
}