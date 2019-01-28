using System.Web.Http;
using System.Web.Routing;
using Telemetry.Api.DelegatingHandlers;
using Telemetry.Core;


namespace Telemetry.Api
{

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "device-events",
                routeTemplate: "events",
                defaults: new { controller = "Events" },
                handler: new GZipToJsonHandler(GlobalConfiguration.Configuration),
                constraints: null);
        }
    }
}
