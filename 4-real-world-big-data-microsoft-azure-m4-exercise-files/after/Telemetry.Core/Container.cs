using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;

namespace Telemetry.Core
{
    public static class Container
    {
        public static IUnityContainer Instance { get; private set; }

        static Container()
        {
            Instance = new UnityContainer();
            RegisterMandatoryComponents();

            RegisterOptionalComponents();
            RegisterConfiguredComponents();
        }

        private static void RegisterMandatoryComponents()
        {
        }

        private static void RegisterConfiguredComponents()
        {
            if (ConfigurationManager.GetSection("unity") != null)
            {
                Instance.LoadConfiguration();
            }
        }

        private static void RegisterOptionalComponents()
        {
        }
    }
}
