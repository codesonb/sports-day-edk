using System;
using System.Collections.Generic;
using System.Linq;
using EDKv5.Statistics;

namespace EDKv5
{
    public abstract partial class Event
    {
        // ---- encapsulated constructor ----
        #region Constructor
        protected Event(string id, string name)
        {
            ID = id;
            Name = name;

            //default
            dic_cp.Add(Group.MA, new CompetitionList());
            dic_cp.Add(Group.MB, new CompetitionList());
            dic_cp.Add(Group.MC, new CompetitionList());
            dic_cp.Add(Group.FA, new CompetitionList());
            dic_cp.Add(Group.FB, new CompetitionList());
            dic_cp.Add(Group.FC, new CompetitionList());

        }
        public static Event Create(int evid, string name, bool assignLanes = true, bool useLongTime = false)
        {
            if (evid <= (int)EventIndex.TrackEventEnd)
                return new TrackEvent(evid.ToString("00"), name, assignLanes, useLongTime);
            else if (evid <= (int)EventIndex.FieldEventEnd)
                return new FieldEvent(evid.ToString("00"), name);
            else if (evid <= (int)EventIndex.SwimGalaEnd)
                return new SwimEvent(evid.ToString("00"), name, assignLanes, useLongTime);
            else
                return new TrackEvent(evid.ToString("00"), name, assignLanes, useLongTime);
        }
        #endregion
        // ---- encapsulated constructor ----

        #region Fields
        SortedList<Group, CompetitionList>
            dic_cp = new SortedList<Group, CompetitionList>();

        public string ID { get; }
        public string Name { get; set; }
        public bool NeedLaneAssignment { get; set; }
        public bool IsTrack { get { return !IsField; } }
        public virtual bool IsSwim { get { return false; } }
        #endregion

        #region Abstraction
        public abstract ICompetitionResultType ResultType { get; }
        public abstract bool IsField { get; }
        #endregion

        #region Get Objects
        #region Properties - Open Groups
        public Group[] OpenedGroups { get { return dic_cp.Keys.ToArray(); } }
        #endregion
        #region Properties - Participations
        public ParticipationStatistic ParticipationStatistic
        {
            get
            {
                Project project = Project.GetInstance();
                ParticipationStatistic stat = new ParticipationStatistic();

                //get reference
                Group[] allGrp = Groups.All;
                House[] allHs = project.Houses;

                Class[] cls = project.Classes;
                string[] allCls = new string[cls.Length];
                for (int i = 0; i < cls.Length; i++)
                    allCls[i] = cls[i].Key;

                //init
                stat.Group = new int[allGrp.Length];
                stat.House = new int[allHs.Length];
                stat.Class = new int[allCls.Length];

                foreach (CompetitionList cps in dic_cp.Values)
                {
                    foreach (Participant p in cps.Participants)
                    {
                        int idx;
                        //count group
                        idx = Array.IndexOf(allGrp, p.Group);
                        if (idx >= 0) stat.Group[idx]++;

                        //count house
                        idx = Array.IndexOf(allHs, p.House);
                        if (idx >= 0) stat.House[idx]++;

                        //count class
                        idx = Array.IndexOf(allCls, p.ClassName);
                        if (idx >= 0) stat.Class[idx]++;

                        stat.Total++;
                    }
                }
                return stat;

            }
        }
        public Tuple<Group, Participant[]>[] Participants
        {
            get
            {
                List<Tuple<Group, Participant[]>> rtn = new List<Tuple<Group, Participant[]>>();

                foreach (KeyValuePair<Group, CompetitionList> kvp in dic_cp)
                    rtn.Add(new Tuple<Group, Participant[]>(kvp.Key, kvp.Value.Participants));

                return rtn.ToArray();
            }
        }
        #endregion
        #region Properties - Lanes Assignment
        public bool IsLaneAssigned
        {
            get
            {
                if (dic_cp.Count > 0)
                {
                    foreach (CompetitionList itm in dic_cp.Values)
                        if (itm.Competitions.Length <= 0) return false;
                    return true;
                }
                return false;
            }
        }
        #endregion
        #region Properties - Competitions
        public Tuple<Group, ICompetition[]>[] Competitions
        {
            get
            {
                List<Tuple<Group, ICompetition[]>> ls = new List<Tuple<Group, ICompetition[]>>();
                foreach (KeyValuePair<Group, CompetitionList> kvp in dic_cp)
                {
                    Tuple<Group, ICompetition[]> tp =
                        new Tuple<Group, ICompetition[]>(kvp.Key, kvp.Value.Competitions);
                    ls.Add(tp);
                }
                return ls.ToArray();
            }
        }
        #endregion
        #region Function - Competition
        public ICompetition[] GetCompetitions(Group group)
        {
            return _i_getCompetitions(group);
            //return Array.ConvertAll(_i_getCompetitions(group), (x) => (ICompetition)x );
        }
        internal Competition[] _i_getCompetitions(Group group)
        {
            CompetitionList val;

            if (dic_cp.TryGetValue(group, out val))
            {
                return val.Competitions;
            }

            throw new Exception("Group is not openned for competition.");
        }
        #endregion
        #endregion

        #region Openning Group Settings
        public void OpenGroup(Group group)
        {
            foreach (Group g in dic_cp.Keys)
            {
                if ((byte)(g & group) > 0)
                    throw new InvalidOperationException("Cannot open the same groups");
            }
            dic_cp.Add(group, new CompetitionList());
        }
        public void UpdateGroup(Group origin, Group modify)
        {
            CompetitionList ls;
            if (dic_cp.TryGetValue(origin, out ls))
            {
                dic_cp.Remove(origin);
                dic_cp.Add(modify, ls);
            }
        }
        public void UpdateGroup(int index, Group modify)
        {
            UpdateGroup(dic_cp.Keys.ElementAt(index), modify);
        }
        public void CloseGroup(Group group) { dic_cp.Remove(group); }
        public void CloseAllGroups() { dic_cp.Clear(); }

        public Group Contains(Group group)
        {
            foreach (Group grp in dic_cp.Keys)
                if ((grp & group) > 0) return grp;
            return Group.None;
        }
        #endregion

        #region Participation
        public bool Contains(Participant participant)
        {
            Group grp = participant.Group;
            Group opGrp = Contains(grp);

            //case 1
            if (Group.None == opGrp) return false;

            //case 2
            CompetitionList ls = dic_cp[opGrp];
            return ls.Contains(participant);
        }
        public bool Join(Participant participant)
        {
            Project prj = Project.GetInstance();
            if (prj.IsJoinLocked)
                throw new InvalidOperationException("Join period is closed.");

            //--------------- normal join
            bool b;
            CompetitionList ls_cp;

            //get actual stored group set
            Group actual = this.Contains(participant.Group);

            //pass and get // for Group.None return false
            if (b = dic_cp.TryGetValue(actual, out ls_cp))      // dirty code
            {
                //check status lanes
                if (ls_cp.IsClosed) { ls_cp.Competitions = null; } // which reset the state back to temporary
                //add operation
                if (!ls_cp.Contains(participant))
                    ls_cp.Add(participant);
            }
            return b;
        }
        public void ClearParticipants()
        {
            foreach (CompetitionList lsCp in dic_cp.Values)
                lsCp.ClearParticipants();
        }
        #endregion

        #region Lane Assignement
        public void AssignLanes()
        {
            Group[] grps = dic_cp.Keys.ToArray(); //copy key list for iteration
            foreach (Group grp in grps)
                AssignLanes(grp);
        }
        public void AssignLanes(Group group)
        {
            CompetitionList iecl = dic_cp[group];
            Competition[] cps = assignLanes(group, iecl.Participants);
            iecl.Competitions = cps;
        }
        //auto lane assignment
        private Competition[] assignLanes(Group group, Participant[] participants)
        {
            if (NeedLaneAssignment && participants.Length > 8)
            {
                //temp list copy participants
                List<Participant> t_ls_cp_ppt = participants.ToList();

                //temp list prepared participants
                int cntCmps = (int)Math.Ceiling((float)t_ls_cp_ppt.Count / 8);
                int shift = 0;
                List<Participant>[] t_ls_pr_ppt = new List<Participant>[cntCmps];
                for (int i = 0; i < t_ls_pr_ppt.Length; i++)
                    t_ls_pr_ppt[i] = new List<Participant>();

                //random generator
                Random r = new Random();

                while (t_ls_cp_ppt.Count > 0)
                {
                    //get random index
                    int rIdx = r.Next(0, t_ls_cp_ppt.Count);

                    //move reference
                    t_ls_pr_ppt[shift].Add(t_ls_cp_ppt[rIdx]);
                    t_ls_cp_ppt.RemoveAt(rIdx);

                    //shift next competition
                    shift = (shift + 1) % cntCmps;
                }

                //temp ls competitions
                List<Competition> t_ls_cmps = new List<Competition>();

                { //create compeitions
                    int i = 1;
                    foreach (List<Participant> ls in t_ls_pr_ppt)
                        t_ls_cmps.Add(new Competition(this, group, i++, ls.ToArray()));
                }

                //output
                return t_ls_cmps.ToArray();
            }
            else
            {
                return new Competition[] { new Competition(this, group, 1, participants) };
            }
        }
        #endregion
    } // end class
}
