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
        interface _I_CompetitionList
        {
            bool IsClosed { get; }
            void Add(CompetitionList competitionList, Participant participant);
            bool Contains(CompetitionList competitionList, Participant participant);
        }
    }
}
