using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Telemetry.Api.Tests.Stubs.EventIngestionApi.Controllers
{
    public class EventsController : ApiController
    {
        public static JArray LastEventBatch { get; set; }

        public static HttpRequestHeaders LatestRequestHeaders { get; set; }

        public async Task<HttpResponseMessage> Post(HttpRequestMessage request)
        {
            LatestRequestHeaders = request.Headers;
            var content = await request.Content.ReadAsStringAsync();
            try
            {
                LastEventBatch = JArray.Parse(content);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Created };
            }
            catch
            {
                LastEventBatch = null;                
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
        } 
    }
}
