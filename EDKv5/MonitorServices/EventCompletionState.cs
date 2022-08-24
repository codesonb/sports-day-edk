using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5.MonitorServices
{
    public enum EventCompletionState : int
    {
        None,
        Passed,
        Marking,
        Partial,
        Full,
        Printed,
        WaitRank,
        Hover=Passed
    }
}
