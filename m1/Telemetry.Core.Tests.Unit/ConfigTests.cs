using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Runtime.Caching;

namespace Telemetry.Core.Tests.Unit
{
    [TestClass]
    public class ConfigTests
    {

        [TestMethod]
        public void Get()
        {
            var environment = Config.Get("EnvironmentName");
            Assert.AreEqual("dev", environment);

            var missing = Config.Get("missing");
            Assert.AreEqual(null, missing);
        }

        [TestMethod]
        public void Get_ShouldCache()
        {
            var cacheGetter = typeof(Config).GetField("_Cache", BindingFlags.NonPublic | BindingFlags.Static);
            var cache = (MemoryCache)cacheGetter.GetValue(typeof(Config));
            var cacheCount = cache.GetCount();

            var aString = Config.Get("AString");
            Assert.AreEqual("string", aString);
            Assert.AreEqual(cacheCount + 1, cache.GetCount());

            aString = Config.Get("AString");
            Assert.AreEqual("string", aString);
        }

        [TestMethod]
        public void Parse_Bool()
        {
            var doLogging = Config.Parse<bool>("LoggingEnabled");
            Assert.IsTrue(doLogging);

            var missing = Config.Parse<bool>("missing");
            Assert.IsFalse(missing);
        }

        [TestMethod]
        public void Parse_TimeSpan()
        {
            var configTimespan = Config.Parse<TimeSpan>("ConfigCacheTimespan");
            Assert.AreEqual(1, configTimespan.TotalSeconds);

            var missing = Config.Parse<TimeSpan>("missing");
            Assert.AreEqual(0, missing.TotalMilliseconds);
        }

        [TestMethod]
        public void Parse_Int()
        {
            var anInt = Config.Parse<int>("AnInt");
            Assert.AreEqual(150, anInt);

            var missing = Config.Parse<int>("missing");
            Assert.AreEqual(0, missing);
        }
    }
}
