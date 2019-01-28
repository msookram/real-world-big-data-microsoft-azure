using dashing.net.common;
using dashing.net.streaming;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Telemetry.Core;
using Telemetry.Entities;

namespace dashing.net.Jobs
{
    [Export(typeof(IJob))]
    public class EventsBreakdownJob : IJob
    {
        public Lazy<Timer> Timer { get; private set; }
        private static long? _EventCount;

        public EventsBreakdownJob()
        {
            Timer = new Lazy<Timer>(() => new Timer(SendEventMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)));
        }

        private void SendEventMessages(object state)
        {
            SendTotalCount();
            SendTopEvents();
            GroupBreakdown("system.app", "events-app");
            GroupBreakdown("device.audio", "events-audio");
            GroupBreakdown("device.wifi", "events-wifi");
            GroupBreakdown("device.gps", "events-gps");
        }

        private static void SendTotalCount()
        {
            var period = DateTime.UtcNow.ToDayPeriod();

            var dbFactory = Container.Instance.Resolve<EventsDbContextFactory>();
            using (var context = dbFactory.GetContext())
            {
                _EventCount = context.EventMetrics.Where(x => x.Period.StartsWith(period))
                                                 .Sum(x => x.Count);
            }

            var message = new
            {
                id = "events-totalcount",
                current = _EventCount
            };
            Dashing.SendMessage(message);
        }

        protected void SendTopEvents()
        {
            var events = new Dictionary<string, long?>();
            var period = DateTime.UtcNow.ToDayPeriod();

            var dbFactory = Container.Instance.Resolve<EventsDbContextFactory>();
            using (var context = dbFactory.GetContext())
            {
                var topEvents = context.EventMetrics.Where(x => x.Period.StartsWith(period))
                                                  .GroupBy(x => x.EventName)
                                                  .Select(g => new
                                                  {
                                                      EventName = g.Key,
                                                      Count = g.Sum(m => m.Count)
                                                  })
                                                  .OrderByDescending(x => x.Count)
                                                  .Take(5);

                foreach (var topEvent in topEvents)
                {
                    events.Add(topEvent.EventName, topEvent.Count);
                }
            }

            events.Add("(other)", _EventCount - events.Values.Sum());

            var pieData = events.Select(x => new
            {
                label = x.Key.Substring(x.Key.IndexOf(".") + 1),
                value = x.Value
            });
            var pieMessage = new
            {
                value = pieData,
                id = "events-top"
            };
            Dashing.SendMessage(pieMessage);

            var listData = events.Select(x => new
            {
                label = x.Key,
                value = x.Value
            });
            var listMessage = new
            {
                items = listData,
                id = "events-toplist"
            };
            Dashing.SendMessage(listMessage);
        }

        protected void GroupBreakdown(string groupPrefix, string widgetId)
        {
            var events = new Dictionary<string, long?>();
            var period = DateTime.UtcNow.ToDayPeriod();
            long? allEvents;

            var dbFactory = Container.Instance.Resolve<EventsDbContextFactory>();
            using (var context = dbFactory.GetContext())
            {
                allEvents = context.EventMetrics.Count(x => x.Period.StartsWith(period) && x.EventName.StartsWith(groupPrefix));
                var topEvents = context.EventMetrics.Where(x => x.Period.StartsWith(period) && x.EventName.StartsWith(groupPrefix))
                                                  .GroupBy(x => x.EventName)
                                                  .Select(g => new
                                                  {
                                                      EventName = g.Key,
                                                      Count = g.Sum(m => m.Count)
                                                  })
                                                  .OrderByDescending(x => x.Count)
                                                  .Take(6);

                foreach (var topEvent in topEvents)
                {
                    events.Add(topEvent.EventName, topEvent.Count);
                }
            }

            if (events.Count > 5)
            {
                events.Remove(events.Last().Key);
                events.Add("(other)", allEvents - events.Values.Sum());
            }

            var pieData = events.Select(x => new
            {
                label = x.Key.Substring(x.Key.LastIndexOf(".") + 1),
                value = x.Value
            });
            var pieMessage = new
            {
                value = pieData,
                id = widgetId
            };
            Dashing.SendMessage(pieMessage);
        }
    }
}