using Microsoft.Practices.Unity;
using Telemetry.Core;
using Telemetry.Entities;

namespace dashing.net.App_Start
{
    public class ContainerConfig
    {
        public static void RegisterComponents()
        {
            var dbConnectionString = Config.Get("EventsDb.ConnectionString");
            Container.Instance.RegisterType<EventsDbContextFactory>(new InjectionConstructor(dbConnectionString));
        }
    }
}