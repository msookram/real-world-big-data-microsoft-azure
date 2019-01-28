using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using Telemetry.Core;

namespace Telemetry.Api.Tests.Unit
{
    public static class ConfigChanger
    {
        private static List<string> _ChangedKeys = new List<string>();

        public static void Set(string key, string value)
        {
            var cache = GetConfigCache();
            cache.Set(key, value, new CacheItemPolicy());
            _ChangedKeys.Add(key);
        }

        public static void Reset()
        {
            var cache = GetConfigCache();
            foreach (var key in _ChangedKeys)
            {
                cache.Remove(key);
            }
        }

        private static MemoryCache GetConfigCache()
        {

            var cacheAccessor = typeof(Config).GetField("_Cache", BindingFlags.NonPublic | BindingFlags.Static);
            var cache = (MemoryCache)cacheAccessor.GetValue(typeof(Config));
            return cache;
        }
    }
}