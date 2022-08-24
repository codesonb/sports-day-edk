using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5
{
    public abstract partial class Event
    {
#if TEST
        public
#endif
        sealed class _Temporary_CompetitionListState : _I_CompetitionList
        {
            //constructor
            // -- public no-argument constructor

            public Event Event { get; private set; }
            public bool IsResultCreated { get { return false; } }

            public bool IsClosed { get { return false; } }

            public void Add(CompetitionList cmpLs, Participant participant)
            {
                cmpLs._ls.Add(participant);
            }
            public bool Contains(CompetitionList cmpLs, Participant participant)
            {
                return cmpLs._ls.Contains(participant);
            }
        }

    }
}
