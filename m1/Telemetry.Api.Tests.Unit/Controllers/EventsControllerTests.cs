using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Telemetry.Api.Analytics;
using Moq;
using Telemetry.Api.Controllers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Caching;

namespace Telemetry.Api.Tests.Unit.Controllers
{
    [TestClass]
    public class EventsControllerTests
    {
        private Mock<IEventSender> _senderMock;

        [TestMethod]
        public async Task Post_BadJson_ShouldReturn400()
        {
            var request = new HttpRequestMessage();
            request.Headers.Add("x-device-id", "abc123");
            request.Content = new StringContent("{\"events\" : [}");

            var controller = GetController(request);

            var response = await controller.Post(request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _senderMock.Verify(x => x.SendEventsAsync(It.IsAny<JArray>(), It.IsAny<string>()), Times.Never);
       
        }

        [TestMethod]
        public async Task Post_Event_ShouldReturn201()
        {
            var events = new
            {
                events = new[] {
                    new {
                        deviceId = "def234",
                        eventName = "device.activated",
                        timestamp = 1415807587310
                    }
                }
            };

            var request = new HttpRequestMessage();
            request.Headers.Add("x-device-id", "def234");
            request.Content = new StringContent(JsonConvert.SerializeObject(events));

            var controller = GetController(request);
            _senderMock.Setup(x => x.SendEventsAsync(It.IsAny<JArray>(), It.IsAny<string>()))
                       .Returns(Task.Delay(100)); //delay to ensure the task doesn't complete before the await

            var response = await controller.Post(request);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            _senderMock.Verify(x => x.SendEventsAsync(It.Is<JArray>(e => e.Count == 1), "def234"), Times.Once);
        }

        [TestMethod]
        public async Task Post_SenderFails_ShouldReturn500()
        {
            var request = new HttpRequestMessage();
            request.Headers.Add("x-device-id", "ghi345");
            request.Content = new StringContent("{\"events\" : []}");

            var controller = GetController(request);
            _senderMock.Setup(x => x.SendEventsAsync(It.IsAny<JArray>(), It.IsAny<string>()));

            var response = await controller.Post(request);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            _senderMock.Verify(x => x.SendEventsAsync(It.Is<JArray>(e => e.Count == 0), "ghi345"), Times.Once);
        }

        [TestMethod]
        public async Task Post_SenderException_ShouldReturn500()
        {
            var request = new HttpRequestMessage();
            request.Headers.Add("x-device-id", "ghi345");
            request.Content = new StringContent("{\"events\" : []}");

            var controller = GetController(request);
            _senderMock.Setup(x => x.SendEventsAsync(It.IsAny<JArray>(), It.IsAny<string>()))
                       .Throws<InvalidOperationException>();

            var response = await controller.Post(request);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            _senderMock.Verify(x => x.SendEventsAsync(It.Is<JArray>(e => e.Count == 0), "ghi345"), Times.Once);
        }

        [TestMethod]
        public async Task Post_SendDisabled_ShouldReturn400()
        {
            ConfigChanger.Set("Telemetry.DeviceEvents.SendToEventHubs", "false");

            var request = new HttpRequestMessage();
            request.Headers.Add("x-device-id", "abc123");
            request.Content = new StringContent("{\"events\" : []}");

            var controller = GetController(request);

            var response = await controller.Post(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _senderMock.Verify(x => x.SendEventsAsync(It.IsAny<JArray>(), It.IsAny<string>()), Times.Never);

            ConfigChanger.Reset();

        }

        private EventsController GetController(HttpRequestMessage request)
        {
            _senderMock = new Mock<IEventSender>();
            var controller = new EventsController(_senderMock.Object)
            {
                Request = request,
                Configuration = new HttpConfiguration()
            };
            return controller;
        }
    }
}
