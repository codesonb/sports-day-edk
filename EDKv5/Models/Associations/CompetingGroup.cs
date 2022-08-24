using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace EDKv5
{
    public class CompetingGroup : IComparable<CompetingGroup>
    {
        public static Func<CompetingGroup, string> GetNameMethod = (c) =>
        {
            return c.Group.ToString() + " - " + c.Event.Name;
        };

        public CompetingGroup(int tempId, Event @event, Group group)
        {
            ID = tempId;
            Event = @event;
            Group = group;
        }

        public int ID { get; }
        public Event Event { get; }
        public Group Group { get; }

        public string Name { get { return GetNameMethod(this); } }

        public int CompareTo(CompetingGroup other)
        {
            return ID.CompareTo(other.ID);
        }

        public override string ToString()
        {
            return this.Name;
        }

    }
}
