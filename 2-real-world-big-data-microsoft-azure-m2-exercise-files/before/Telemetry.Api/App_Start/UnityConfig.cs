using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Configuration;

namespace Telemetry.Api.App_Start
{
    public class UnityConfig
    {
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterDefaults(container);
            RegisterConfig(container);
            return container;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }

        private static void RegisterConfig(IUnityContainer container)
        {
            if (ConfigurationManager.GetSection("unity") != null)
            {
                container.LoadConfiguration();
            }
        }

        private static void RegisterDefaults(IUnityContainer container)
        {

        }
    }
}
