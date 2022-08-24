using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDKv5;
using EDKv5.Statistics;

namespace Launcher.Algorithms
{
    class ScoreStatistic : EDKv5.Statistics.ScoreStatistic
    {
        public ScoreStatistic(Project project) : base(project)
        {
            prj = project;
        }
        Project prj;

        public Score<House>[] Houses { get { return GetScore(_dicHs);} }
        public Score<Class>[] Classes { get { return GetScore(_dicCls); } }
        public Score<Student>[] Individuals { get { return GetScore(_dicStu); } }
        public GroupRanks[] Groups
        {
            get
            {
                var list = new List<GroupRanks>();
                foreach (var kvpGrpDict in _dicGrp)
                {
                    var group = kvpGrpDict.Key;
                    var dict = kvpGrpDict.Value;

                    var grpRnks = new GroupRanks(group);
                    var i = 0;
                    foreach (var kvpScoreStu in dict)
                    {
                        if (++i > 8) break;
                        grpRnks.Add(kvpScoreStu.Value, kvpScoreStu.Key);
                    }
                    list.Add(grpRnks);
                    
                }
                return list.ToArray();
            }
        }

        private Score<T>[] GetScore<T>(Dictionary<T, Score> dic)
        {
            var list = new List<Score<T>>();
            foreach (var kvp in dic)
                list.Add(new Score<T>(kvp.Key, kvp.Value));
            list.Sort((Score<T> a, Score<T> b) => { return b.Value.Total.CompareTo(a.Value.Total); });

            int rank = 0;
            int gap = 1;
            int lastScore = list[0].Value.Total + 1;
            foreach (var item in list)
            {
                int thisTotal = item.Value.Total;
                if (lastScore > thisTotal)
                {
                    rank += gap;
                    gap = 1;
                }
                else
                {
                    gap++;
                }
                item.Rank = rank;
                lastScore = thisTotal;
            }
            return list.ToArray();
        }

    }
    class Score<T>: IComparable<Score<T>>
    {
        public Score(T house, Score score)
        {
            this.Key = house;
            this.Value = score;
        }
        public T Key { get; }
        public Score Value { get; }
        public int Rank { get; set; }

        public int CompareTo(Score<T> other)
        {
            return other.Value.CompareTo(Value);
        }
    }
    class GroupRanks
    {
        public GroupRanks(Group group)
        {
            this.Group = group;
        }

        List<StudentScoreCollection> top8 = new List<StudentScoreCollection>();
        public Group Group { get; }
        public object Gold { get { return _getRanked(0); } }
        public object Silver { get { return _getRanked(1); } }
        public object Brownz { get { return _getRanked(2); } }
        public object th4 { get { return _getRanked(3); } }
        public object th5 { get { return _getRanked(4); } }
        public object th6 { get { return _getRanked(5); } }
        public object th7 { get { return _getRanked(6); } }
        public object th8 { get { return _getRanked(7); } }

        private object _getRanked(int k)
        {
            int n = 0;
            for (int i = 0; i < top8.Count; i++)
            {
                if (i + n == k) return top8[i];
                n += top8[i].Count - 1;
            }
            return "";
        }

        public void Add(List<Student> students, Score score)
        {
            var ls = new StudentScoreCollection();
            foreach (var stu in students)
                ls.Add(new StudentScore(stu, score));
            top8.Add(ls);
        }

    }
    struct StudentScore
    {
        public StudentScore(Student student, Score score) { this.Student = student; this.Score = score; }
        public Student Student { get; set; }
        public Score Score { get; set; }
        public string DisplayValue
        {
            get { return string.Format("{0}-{1} ({2})", Student.ClassName, Student.Name, Score.Total); }
        }
    }
    class StudentScoreCollection : List<StudentScore> { }

}
