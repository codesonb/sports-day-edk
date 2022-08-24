using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    public abstract class Participant
    {
        public abstract string ID { get; }
        public abstract string Name { get; }
        public Group Group { get; protected set; }
        public House House { get; protected set; }
        public abstract string ClassName { get; }

        public Event[] ParticipatedEvents
        {
            get
            {
                Project project = Project.GetInstance();
                Event[] evs = project.Events;
                List<Event> ls = new List<Event>();

                foreach (Event ev in evs)
                {
                    if (ev.Contains(this)) { ls.Add(ev); }
                }
                return ls.ToArray();
            }
        }
    }
}
