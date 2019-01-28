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
    public class EventsSummaryJob : IJob
    {
        public Lazy<Timer> Timer { get; private set; }        

        public EventsSummaryJob()
        {
            Timer = new Lazy<Timer>(() => new Timer(SendEventMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)));
        }

        private void SendEventMessages(object state)
        {
            var todayPeriod = DateTime.UtcNow.ToDayPeriod();
            var thisMonthPeriod = DateTime.UtcNow.ToMonthPeriod();

            SendSummary("events-todayhours", todayPeriod);
            SendSummary("events-monthhours", thisMonthPeriod, todayPeriod);
        }

        private void SendSummary(string widgetId, string includePeriod, string excludePeriod = "")
        {
            var events = new Dictionary<string, long?>();
            for (int i = 0; i < 24; i++)
            {
                events.Add(i.ToString("0#"), 0);
            }

            var daysWithData = 1;
            var dbFactory = Container.Instance.Resolve<EventsDbContextFactory>();
            using (var context = dbFactory.GetContext())
            {
                var eventsByHour = context.EventMetrics.Where(x => x.Period.StartsWith(includePeriod) && 
                                                                   (string.IsNullOrEmpty(excludePeriod) || !x.Period.StartsWith(excludePeriod)))
                                                     .Select(x => new { Period = x.Period, Count = x.Count })
                                                     .GroupBy(x => x.Period)
                                                     .Select(g => new { Period = g.Key, Count = g.Sum(m => m.Count) });

                foreach (var hourEvents in eventsByHour)
                {
                    var hour = hourEvents.Period.Substring(8, 2);
                    events[hour] = events[hour] + hourEvents.Count;
                }

                daysWithData = eventsByHour.Select(x => x.Period.Substring(6, 2)).Distinct().Count();
            }

            var data = events.Select(evt => new
            {
                x = GetHourAsUnixSeconds(evt.Key),
                y = evt.Value / daysWithData
            });
            var message = new
            {
                points = data,
                current = data.Sum(x => x.y),
                displayedValue = data.Sum(x => x.y),
                id = widgetId
            };
            Dashing.SendMessage(message);
        }

        private static long GetHourAsUnixSeconds(string hour)
        {
            var utcToday = DateTime.UtcNow.Date;
            var hourDate = new DateTime(utcToday.Year, utcToday.Month, utcToday.Day, int.Parse(hour), 0, 0);
            return hourDate.ToUnixSeconds();
        }
    }
}