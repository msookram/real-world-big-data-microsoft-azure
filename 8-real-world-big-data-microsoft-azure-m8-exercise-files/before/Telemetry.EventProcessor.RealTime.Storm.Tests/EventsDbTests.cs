using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace Telemetry.EventProcessor.RealTime.Storm.Tests
{
    [Ignore] //manual integration test
    [TestClass]
    public class EventsDbTests
    {
        [TestMethod]
        public void MergeEventMetric()
        {
            using (var db = new EventsDb(ConfigurationManager.AppSettings["EventsDb.ConnectionString"]))
            {
                db.MergeEventMetric("EventsDbTests.UpsertEventMetric", "2015060314", 100);
                db.MergeEventMetric("EventsDbTests.UpsertEventMetric", "2015060314", 50);
                db.MergeEventMetric("EventsDbTests.UpsertEventMetric", "2015060315", 200);
            }
        }
    }
}
