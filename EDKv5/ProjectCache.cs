using System;
using System.Collections.Generic;

namespace EDKv5
{
    static class ProjectCache
    {
        private static Dictionary<int, ICompetition> _l_comps;
        private static Dictionary<int, ICompetition> CompetitionCache
        {
            get
            {
                // lazy initialization
                if (null == _l_comps)
                {
                    _l_comps = new Dictionary<int, ICompetition>();
                    int _evg_id = 1;
                    Project prj = Project.GetInstance();
                    foreach (Event ev in prj.Events)
                    {
                        foreach (Tuple<Group, ICompetition[]> tup in ev.Competitions)
                        {
                            _evg_id += 100;
                            int _evg_cmp_id = _evg_id;
                            foreach (ICompetition icmp in tup.Item2)
                                _l_comps.Add(_evg_cmp_id++, icmp);
                        }
                    }
                }
                return _l_comps;
            }
        } // end of [List<ICompetition> CompetitionCache]

        public static ICompetition GetCompetitionByID(int id)
        {
            return CompetitionCache[id];
        }

        public static int ID(this ICompetition comp)
        {
            foreach (KeyValuePair<int, ICompetition> kvp in CompetitionCache)
            {
                if (object.Equals(kvp.Value, comp)) return kvp.Key;
            }
            throw new Exception("ID is not found");
        }
    }
}
