using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace Telemetry.Api.Tests.Stubs.EventIngestionApi
{
    public class SelfHost
    {
        private static CancellationTokenSource _ServerCancellationTokenSource;
        public static Exception ServerStartException;
        public static HttpSelfHostServer Server;

        public static void Start()
        {
            ServerStartException = null;
            _ServerCancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => StartInternal(), _ServerCancellationTokenSource.Token);
        }

        public static void StartInternal()
        {
            var baseAddress = "http://localhost:8888/";
            var selfHostconfiguration = new HttpSelfHostConfiguration(baseAddress);
            //allow to run non-elevated:
            selfHostconfiguration.HostNameComparisonMode = HostNameComparisonMode.Exact; 
            //for large message testing:
            selfHostconfiguration.MaxReceivedMessageSize = 500 * 1024; 
            selfHostconfiguration.Routes.MapHttpRoute(
                name: "Deafult API",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            Server = new HttpSelfHostServer(selfHostconfiguration);
            var serverTask = Server.OpenAsync();
            serverTask.Wait();
            if (serverTask.IsFaulted)
            {
                ServerStartException = serverTask.Exception;
            }
        }

        public static void Stop()
        {
            Server.CloseAsync().Wait();
            Server = null;
            _ServerCancellationTokenSource.Cancel();
        }
    }
}
