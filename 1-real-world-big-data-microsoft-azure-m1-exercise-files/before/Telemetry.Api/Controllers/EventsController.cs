using NLog;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telemetry.Api.Analytics;

namespace Telemetry.Api.Controllers
{   
    public class EventsController : ApiController
    {
        private readonly Logger _log;

        public EventsController()
        {
            _log = this.GetLogger();
        }

        public async Task<HttpResponseMessage> Post(HttpRequestMessage requestMessage)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}