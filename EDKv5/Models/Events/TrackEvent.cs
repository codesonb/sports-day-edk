using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    partial class TrackEvent : Event
    {
        public TrackEvent(string ID, string name, bool assignLane, bool useLongTime) : base(ID, name)
        {
            NeedLaneAssignment = assignLane;
            UseLongTime = useLongTime;
        }

        public override bool IsField { get { return false; } }
        public bool UseLongTime = false;

        public override ICompetitionResultType ResultType
        {
            get
            {
                if (UseLongTime)
                    return LongTimeResultType.GetInstance();
                else
                    return ShortTimeResultType.GetInstance();
            }
        }
    }
}
