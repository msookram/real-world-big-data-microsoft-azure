using Microsoft.WindowsAzure;
using System;
using System.Globalization;
using System.Runtime.Caching;

namespace Telemetry.Core
{
    public static class Config
    {
        private static MemoryCache _Cache;
        private static TimeSpan _CacheLifespan;

        static Config()
        {
            _CacheLifespan = TimeSpan.Parse(CloudConfigurationManager.GetSetting("ConfigCacheTimespan"));
            _Cache = new MemoryCache("Telemetry.Config.Cache");
        }

        public static T Parse<T>(string name) 
            where T : IComparable
        {
            IComparable value = default(T);
            var valueString = Get(name);
            if (valueString != null)
            {
                if (value is TimeSpan)
                {
                    value = TimeSpan.Parse(valueString);
                }
                else if (value is int)
                {
                    value = int.Parse(valueString);
                }
                else if (value is bool)
                {
                    value = bool.Parse(valueString);
                }
            }
            return (T) value;
        }

        public static string Get(string name)
        {
            string item = null;
            
            if (!string.IsNullOrEmpty(name))
            {
                if (_Cache.Contains(name))
                {
                    item = (string)_Cache[name];
                }
                else
                {
                    item = CloudConfigurationManager.GetSetting(name);

                    if (_CacheLifespan.TotalSeconds > 0 && !string.IsNullOrEmpty(item))
                    {
                        var cacheItem = new CacheItem(name, item);
                        var policy = new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.Add(_CacheLifespan)
                        };
                        _Cache.Add(cacheItem, policy);
                    }
                }
            }

            return item;
        }
    }
}
