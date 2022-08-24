using EDKv5;

using System;
using System.Collections.Generic;
using EDKv5.Protocols;

using static EDKv5.ExtendsResultType;

namespace EDKv5
{
#if TEST
    public  // set to public for easy creation in Unit Test project
#endif
    class Competition : ICompetition
    {
        public Competition(Event ev, Group group, int index, Participant[] participants)
        {
            Event = ev;
            Group = group;
            Index = index;
            _ls_ppt = new List<Participant>(participants);
        }

        //fields
        Dictionary<Participant, CompetitionResult> _ls_results;
        List<Participant> _ls_ppt;

        //properties
        public Event Event { get; }
        public Group Group { get; }
        public int Index { get; }
        public List<Participant> Participants { get { return _ls_ppt; } }
        public CompetitionResult[] Results
        {
            get
            {
                var tmp = new List<CompetitionResult>(_ls_results.Values);
                tmp.Sort((CompetitionResult a, CompetitionResult b) => {
                    if (0 == a.Value) return 1;
                    if (0 == b.Value) return -1;
                    return a.Rank.CompareTo(b.Rank);
                });
                return tmp.ToArray();
            }
        }
        public int ParticipantsCount { get { return _ls_ppt.Count; } }

        public bool IsResultCreated
        {
            get { return null != _ls_results && 0 < _ls_results.Count; }
        }
        public bool IsCompleted { get; private set; }
        public bool IsRankMatched { get; private set; }

        internal void _i_updateResults(CompetitionResult[] results)
        {
            //create if not exists
            if (this.IsResultCreated)
            {
                for (int i = 0; i < results.Length; i++)
                {
                    CompetitionResult result = _ls_results[_ls_ppt[i]];
                    result.State = results[i].State;
                    result.Value = results[i].Value;
                    result.Rank = results[i].Rank;
                }
            }
            else
            {
                _ls_results = new Dictionary<Participant, CompetitionResult>();

                for (int i = 0; i < results.Length; i++)
                {
                    Participant ppt = _ls_ppt[i];
                    _ls_results.Add(ppt, results[i]);
                    results[i].Participant = ppt;
                }
            }

            // call to sort ranks
            CompetitionResult.ComputeRanks(_ls_results.Values);
            _ls_results.Values.CopyTo(results, 0);

            // compare the entered and computed results
            this.IsCompleted = checkCompleted();

            // ~ check all rank is entered
            foreach (var result in results)
            {
                if (0 != result.Rank)
                {
                    this.IsRankMatched = compareResults(results);
                    return;
                }
            }

            // ~ use computed rank as input rank if no rank is entered
            foreach (var result in results)
                result.Rank = result.ComputedRank;
            this.IsRankMatched = true;

        }
        private bool checkCompleted()
        {
            if (!this.IsResultCreated) return false;
            foreach (CompetitionResult result in _ls_results.Values)
                if (0 == result.Value && ResultState.Rank == result.State) { return false; }    // zero is not allowed
            return true;
        }
        private bool compareResults(CompetitionResult[] results)
        {
            for (int i = 0; i < results.Length; i++)
                if (results[i].Rank != results[i].ComputedRank) { return false; }
            return true;
        }

        public void Remove(Participant participant)
        {
            _ls_ppt.Remove(participant);
        }
        public void Add(Participant participant)
        {
            _ls_ppt.Add(participant);
        }
        public void Insert(int index, Participant participant)
        {
            _ls_ppt.Insert(index, participant);
        }

        ILaneSetting[] ICompetition.CreateLaneSettings()
        {
            bool hasResult = this.IsResultCreated;
            LaneSetting[] arr = new LaneSetting[_ls_ppt.Count];
            for (int laneIdx = 0; laneIdx < arr.Length; laneIdx++)
            {
                Participant ppt = _ls_ppt[laneIdx];
                arr[laneIdx] = new LaneSetting()
                {
                    Lane = (short)(laneIdx + 1),
                    PID = ppt.ID,
                    Name = ppt.Name,
                    HouseColor = ppt.House.Color.ToArgb(),
                };
                if (hasResult)
                {
                    var result = _ls_results[ppt];
                    arr[laneIdx].Value = result.Value;
                    arr[laneIdx].Rank = result.Rank;
                    arr[laneIdx].State = result.State;
                }
            }
            return arr;
        }
    }
}