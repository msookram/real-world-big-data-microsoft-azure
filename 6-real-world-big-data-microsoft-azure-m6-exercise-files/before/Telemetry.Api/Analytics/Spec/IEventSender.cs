using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Telemetry.Api.Analytics
{
    public interface IEventSender
    {
        Task SendEventsAsync(JArray events, string deviceId);
    }
}
