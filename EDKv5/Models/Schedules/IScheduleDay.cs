using System;
using System.Collections.Generic;

namespace EDKv5.SchedulerService
{
    public interface IScheduleDay
    {
        DateTime Start { get; set; }
        IPeriod[] Periods { get; }
        void Add(IPeriod period);
        void Remove(IPeriod period);
        void Clear();
    }
    
}
