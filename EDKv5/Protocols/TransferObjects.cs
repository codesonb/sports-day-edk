using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This file contains the objects (structed data / information)
// that alive in the network traffics. These objects are not
// the actual stored data in the project file or running system.

namespace EDKv5.Protocols
{
    public class CompetitionOutline
    {
        public int CompID { get; set; }
        public Group CompGroup { get; set; }
    }
    public class EventOutline
    {
        // fields
        List<CompetitionOutline> _comp = new List<CompetitionOutline>();

        // properties
        public string ID { get; set; }
        public string EventName { get; set; }
        public CompetitionOutline[] Competitions { get { return _comp.ToArray(); } }
        public bool IsField { get; set; }

        // functions
        public void AddCompetitions(IEnumerable<CompetitionOutline> outlines) { _comp.AddRange(outlines); }
    }

    public interface ILaneSetting
    {
        short Lane { get; }
        string PID { get; }
        string Name { get; }
        int HouseColor { get; }
        uint Value { get; set; }
        short Rank { get; set; }
        ResultState State { get; set; }
    }

    class LaneSetting : ILaneSetting
    {
        public short Lane { get; set; }
        public string PID { get; set; }
        public string Name { get; set; }
        public int HouseColor { get; set; }
        public uint Value { get; set; }
        public short Rank { get; set; }
        public short ComputedRank { get; set; }
        public ResultState State { get; set; }
    }

}
