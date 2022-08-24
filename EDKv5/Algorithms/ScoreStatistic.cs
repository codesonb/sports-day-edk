using System;
using System.Collections.Generic;
using System.Linq;

namespace EDKv5.Statistics
{
    public struct Score : IComparable<Score>
    {
        public static bool operator <(Score a, Score b) { return a.Total < b.Total; }
        public static bool operator >(Score a, Score b) { return a.Total > b.Total; }
        public static bool operator <=(Score a, Score b) { return a.Total <= b.Total; }
        public static bool operator >=(Score a, Score b) { return a.Total >= b.Total; }

        public int Participation { get; set; }
        public int RankScore { get; set; }
        public int RelayDoubled { get; set; }
        public int Deduction { get; set; }
        public int Total { get { return Participation + RankScore - Deduction; } }
        public static Score operator +(Score a, Score b)
        {
            a.Participation += b.Participation;
            a.RankScore += b.RankScore;
            a.RelayDoubled += b.RelayDoubled;
            a.Deduction += b.Deduction;
            return a;
        }

        public int CompareTo(Score other)
        {
            return other.Total.CompareTo(this.Total);
        }
    }
    public class ScoreStatistic
    {
        const int C_BREAK_RECORD_SCORE = 2;
        const int C_CHAMPION_SCORE = 1;

        const int C_ABSENT_DEDUCTION = 1;

        protected Dictionary<House, Score> _dicHs = new Dictionary<House, Score>();
        protected Dictionary<Group, SortedDictionary<Score, List<Student>>> _dicGrp = new Dictionary<Group, SortedDictionary<Score, List<Student>>>();
        protected Dictionary<Class, Score> _dicCls = new Dictionary<Class, Score>();
        protected Dictionary<Student, Score> _dicStu = new Dictionary<Student, Score>();

        public ScoreStatistic(Project project)
        {
            foreach (var hs in project.Houses)
                _dicHs.Add(hs, new Score());
            foreach (var cls in project.Classes)
                _dicCls.Add(cls, new Score());
            foreach (var stu in project.Students)
                _dicStu.Add(stu, new Score());

            _dicGrp.Add(Group.MA, new SortedDictionary<Score, List<Student>>());
            _dicGrp.Add(Group.MB, new SortedDictionary<Score, List<Student>>());
            _dicGrp.Add(Group.MC, new SortedDictionary<Score, List<Student>>());
            //_dicGrp.Add(Group.MD, new Dictionary<Score, List<Student>>());
            _dicGrp.Add(Group.FA, new SortedDictionary<Score, List<Student>>());
            _dicGrp.Add(Group.FB, new SortedDictionary<Score, List<Student>>());
            _dicGrp.Add(Group.FC, new SortedDictionary<Score, List<Student>>());
            //_dicGrp.Add(Group.FD, new Dictionary<Score, List<Student>>());

            // get scores
            EventIndex[] relayIndice = Events.Relay;
            foreach (Event ev in project.Events)
            {
                EventIndex evid = (EventIndex)Enum.Parse(typeof(EventIndex), ev.ID);
                bool isRelay = relayIndice.Contains(evid);
                foreach (var kvp in ev.Competitions)
                {
                    HolderRecord holder = project.GetHolder(evid, kvp.Item1);
                    foreach (ICompetition cmp in kvp.Item2)
                        if (cmp.IsCompleted) _cal_competition((Competition)cmp, holder, isRelay);
                }
            }

            // pass calculated score to group rank
            foreach (var kvpStuScore in _dicStu)
            {
                var student = kvpStuScore.Key;
                var totalScore = kvpStuScore.Value;
                //-- update group ranks
                List<Student> stus;
                SortedDictionary<Score, List<Student>> dict = _dicGrp[student.Group];
                if (totalScore.RankScore > 0)
                { 
                    if (!dict.TryGetValue(totalScore, out stus))
                        dict.Add(totalScore, stus = new List<Student>());
                    stus.Add(student);
                }
            }

        }
        private void _cal_competition(Competition competition, HolderRecord holder, bool isRelay)
        {
            Project prj = Project.GetInstance();
            foreach (var result in competition.Results)
            {
                Score score = new Score();

                // rank score
                switch (result.State)
                {
                    case ResultState.Absent:
                        score.Deduction += C_ABSENT_DEDUCTION;    // absent deduction
                        break;
                    case ResultState.Leave:
                        break;
                    case ResultState.Disqualified:
                        score.Participation += 1;                 // participation
                        break;
                    case ResultState.Rank:
                        score.Participation += 1;                 // pariticipation

                        // break record
                        if (null != holder && holder.Result.CompareTo(result) > 0) { score.RankScore += C_BREAK_RECORD_SCORE; }

                        if (result.Rank < 9)
                        {
                            score.RankScore += (9 - result.Rank);                          // rank score
                            if (1 == result.Rank) { score.RankScore += C_CHAMPION_SCORE; } // champion score
                        }
                        break;
                }
                
                // double relay
                if (isRelay) { score.RelayDoubled = score.RankScore; }

                // count score
                if (0 != score.Total)
                {
                    var ppt = result.Participant;
                    //-- udpate individual score
                    if (ppt is Student)
                    {
                        var stu = (Student)ppt;
                        _dicStu[stu] += score;
                    }
                    else if (ppt is HouseTeam)
                    {
                        var team = (HouseTeam)ppt;
                        foreach (var mem in team.Members)
                            _dicStu[mem] += score;
                    }

                    //-- update class score
                    Class cls;
                    if (prj.TryGetClass(ppt.ClassName, out cls))
                        _dicCls[cls] += score;

                    //-- update house score
                    _dicHs[ppt.House] += score;
                }
            }
        }


    }
}
