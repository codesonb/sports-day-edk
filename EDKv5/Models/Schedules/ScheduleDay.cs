using EDKv5.SchedulerService;
using System;
using System.Collections.Generic;

namespace EDKv5
{
    public partial class Schedule
    {
        private class ScheduleDay : IScheduleDay
        {
            //constructor
            public ScheduleDay(DateTime date)
            {
                this.Start = date;
            }

            //fields
            List<IPeriod> ls_Period = new List<IPeriod>();

            //instance property
            public DateTime Start { get; set; }

            //explict interface implementation
            IPeriod[] IScheduleDay.Periods { get { return ls_Period.ToArray(); } }
            void IScheduleDay.Add(IPeriod period) { ls_Period.Add(period); }
            void IScheduleDay.Remove(IPeriod period) { ls_Period.Remove(period); }
            void IScheduleDay.Clear() { ls_Period.Clear(); }
        }
    }
}
