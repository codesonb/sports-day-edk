using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDKv5.Protocols;

namespace EDKv5
{
    public interface ICompetition
    {
        Event Event { get; }
        Group Group { get; }
        int Index { get; }
        List<Participant> Participants { get; }
        CompetitionResult[] Results { get; }
        int ParticipantsCount { get; }
        bool IsResultCreated { get; }
        bool IsCompleted { get; }
        bool IsRankMatched { get; }

        void Remove(Participant participant);
        void Add(Participant participant);
        void Insert(int index, Participant participant);
        ILaneSetting[] CreateLaneSettings();
    }

}
