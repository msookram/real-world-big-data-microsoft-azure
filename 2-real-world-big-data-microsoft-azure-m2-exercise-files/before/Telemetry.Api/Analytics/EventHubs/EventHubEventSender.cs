using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Telemetry.Api.Analytics.EventHubs
{
    public class EventHubEventSender : IEventSender
    {
        private Logger _log;

        public EventHubEventSender()
        {
            _log = this.GetLogger();
        }

        public Task SendEventsAsync(JArray events, string deviceId)
        {
            throw new NotImplementedException();
        }
    }
}