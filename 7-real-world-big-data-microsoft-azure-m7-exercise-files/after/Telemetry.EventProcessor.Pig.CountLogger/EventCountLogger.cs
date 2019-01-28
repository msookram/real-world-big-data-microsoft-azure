using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using Telemetry.Core.Logging;

namespace Telemetry.EventProcessor.Pig.CountLogger
{
    class EventCountLogger
    {
        public static void Run()
        {
            var log = typeof(Program).GetLogger();
            var eventCounts = new Dictionary<string, long>();

            using (var stdin = Console.OpenStandardInput())
            using (var inputReader = new StreamReader(stdin))
            {
                var line = inputReader.ReadLine();
                while (line != null)
                {
                    var fields = line.Split('\t');
                    var eventName = fields[0];
                    if (eventCounts.ContainsKey(eventName))
                    {
                        eventCounts[eventName]++;
                    }
                    else
                    {
                        eventCounts[eventName] = 1;
                    }
                    line = inputReader.ReadLine();
                }
            }

            using (var stdout = Console.OpenStandardOutput())
            using (var outputWriter = new StreamWriter(stdout))
            {
                foreach (var eventCount in eventCounts)
                {
                    outputWriter.WriteLine(string.Format("{0}\t{1}", eventCount.Key, eventCount.Value));

                    log.InfoEvent("EventCount",
                        new Facet("eventName", eventCount.Key),
                        new Facet("count", eventCount.Value));
                }
            }
        }
    }
}
