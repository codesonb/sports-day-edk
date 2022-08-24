using EDKv5;
using EDKv5.SchedulerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Launcher
{
    public static class ScheduleInitializer
    {
        public static void Init(this Schedule schedule)
        {
            // init days
            DateTime date = DateTime.Now.Date.AddDays(7).AddHours(8).AddMinutes(10);
            IScheduleDay day1 = schedule.CreateDay(date);
            IScheduleDay day2 = schedule.CreateDay(date.AddDays(1));

            // --- Day 1
            day1.Add(new AssemblePeriod(10, Properties.Resources.strAssemble));
            day1.Add(new AssemblePeriod(20, Properties.Resources.strOpenningCeremony));

            day1.Add(new CompetitionPeriod(45));
            day1.Add(new CompetitionPeriod(45));
            day1.Add(new CompetitionPeriod(45));
            day1.Add(new CompetitionPeriod(45));
            day1.Add(new CompetitionPeriod(45));

            day1.Add(new AssemblePeriod(90, Properties.Resources.strLunchTime));

            day1.Add(new CompetitionPeriod(45));
            day1.Add(new CompetitionPeriod(45));
            day1.Add(new CompetitionPeriod(45));

            day1.Add(new AssemblePeriod(15, Properties.Resources.strDismiss));

            // --- Day 2
            day2.Add(new AssemblePeriod(10, Properties.Resources.strAssemble));

            day2.Add(new CompetitionPeriod(45));
            day2.Add(new CompetitionPeriod(45));
            day2.Add(new CompetitionPeriod(45));
            day2.Add(new CompetitionPeriod(45));
            day2.Add(new CompetitionPeriod(45));

            day2.Add(new AssemblePeriod(30, Properties.Resources.strClosingCeremony));
            day2.Add(new AssemblePeriod(15, Properties.Resources.strDismiss));

        }
    }
}
