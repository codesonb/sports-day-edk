using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5
{
    public class HolderRecord
    {
        public HolderRecord(EventIndex @event, Group group, string name, int year, CompetitionResult result)
        {
            Event = @event;
            Group = group;
            Name = name;
            Year = year;
            Result = result;
        }
        public readonly EventIndex Event;
        public readonly Group Group;
        public string Name { get; private set; }
        public int Year { get; private set; }
        public CompetitionResult Result { get; private set; }

        public void Set(string name, int year, CompetitionResult result)
        {
            Name = name;
            Year = year;
            Result = result;
        }
    }
}
