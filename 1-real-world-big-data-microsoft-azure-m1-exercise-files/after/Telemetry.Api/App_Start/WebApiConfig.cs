using System.Net.Http.Formatting;
using System.Web.Http;
using Telemetry.Api.DelegatingHandlers;

namespace Telemetry.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());

            config.Filters.Add(new LoggingExceptionFilterAttribute());
            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new StandardResponseHeadersHandler());
        }
    }
}
