using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    partial class FieldEvent : Event
    {
        public FieldEvent(string ID, string name) :base(ID, name) { }

        public override bool IsField { get { return true; } }

        public override ICompetitionResultType ResultType { get { return LengthResultType.GetInstance(); } }
    }
}
