using System;
using System.Threading.Tasks;

namespace Telemetry.Api.Analytics
{
    public interface IEventSender
    {
        Task SendEventsAsync(Newtonsoft.Json.Linq.JArray events, string deviceId);
    }
}
