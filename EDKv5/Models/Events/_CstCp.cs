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
        sealed class _Assigned_CompetitionListState : _I_CompetitionList
        {
            public bool IsClosed { get { return true; } }

            public void Add(CompetitionList cmpLs, Participant participant)
            {
                throw new InvalidOperationException("Cannot join events after confirmation");
            }

            public bool Contains(CompetitionList cmpLs, Participant participant)
            {
                foreach (Competition cp in cmpLs._cmp)
                    if (cp.Participants.Contains(participant)) return true;
                return false;
            }
        }
    }
}