
using System;
using System.Collections.Generic;
using System.Linq;

using EDKv5.SchedulerService;

namespace EDKv5
{
    public partial class Schedule
    {
        public event EventHandler Updated;

        // constructor
        public Schedule()
        {
            Project prj = Project.GetInstance();
            Event[] evs = prj.Events;

            int idx = 1;
            foreach (Event ev in evs)
            {
                foreach (Group grp in ev.OpenedGroups)
                {
                    CompetingGroup cmpGrp = new CompetingGroup(idx++, ev, grp);
                    ls_cpGrp.Add(cmpGrp);
                }
            }
        }
        // fields
        SortedList<DateTime, ScheduleDay>
            days = new SortedList<DateTime, ScheduleDay>();
        List<CompetingGroup>
            ls_cpGrp = new List<CompetingGroup>();
        Dictionary<CompetingGroup, ICompetingPeriod>
            ls_assigned = new Dictionary<CompetingGroup, ICompetingPeriod>();

        //properties
        public IScheduleDay[] Days { get { return days.Values.ToArray(); } }
        public CompetingGroup[] UnassignedCompetitions
        {
            get { return ls_cpGrp.ToArray(); }
        }

        public IScheduleDay CreateDay(DateTime date)
        {
            ScheduleDay schDay = new ScheduleDay(date);
            days.Add(date, schDay);
            return schDay;
        }

        private bool Assign(ICompetingPeriod period, CompetingGroup cpGrp)
        {
            if (ls_cpGrp.Contains(cpGrp))
            {
                bool rtn;   //return success/fail
                if (rtn = (Group.None == period.Add(cpGrp)))
                {
                    ls_cpGrp.Remove(cpGrp);
                    ls_assigned.Add(cpGrp, period);
                }
                return rtn;
            }
            else
            {
                ICompetingPeriod origin;
                if (ls_assigned.TryGetValue(cpGrp, out origin))
                {
                    // not existance
                    Group matchGroup = period.Add(cpGrp);
                    if (Group.None == matchGroup)
                    {
                        origin.Remove(cpGrp);
                        ls_assigned.Remove(cpGrp);
                        ls_assigned.Add(cpGrp, period);
                        return true;
                    }
                    else if (cpGrp.Group == matchGroup) // check same group and swap
                    {
                        Cell[] cells = period.Cells;
                        for (int i = 0; i < cells.Length; i++)
                        {
                            if (cpGrp.Group == cells[i].Group)
                            {
                                var oriCmpGrp = (CompetingGroup)cells[i].Data;
                                if (cpGrp == oriCmpGrp) { return false; }       // ignore the replace itself
                                // swap same group
                                period.Remove(oriCmpGrp);
                                period.Add(cpGrp);
                                ls_assigned[cpGrp] = period;
                                origin.Remove(cpGrp);
                                origin.Add(oriCmpGrp);
                                ls_assigned[oriCmpGrp] = origin;
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    throw new InvalidOperationException("CompetingGroup does not exists in the schedule context.");
                }
            }
        }

        public int Assign(ICompetingPeriod period, CompetingGroup[] competingGroups)
        {
            if (null == period) return -1;

            int success = 0;
            foreach (CompetingGroup cpGrp in competingGroups)
                if (this.Assign(period, cpGrp)) success++;

            if (success > 0 && null != this.Updated)
                this.Updated(this, EventArgs.Empty);

            return success;
        }

        public bool Revert(CompetingGroup cpGrp)
        {
            ICompetingPeriod period;
            bool b;
            if (b = ls_assigned.TryGetValue(cpGrp, out period))
            {
                period.Remove(cpGrp);
                ls_assigned.Remove(cpGrp);
                ls_cpGrp.Add(cpGrp);

                if (null != this.Updated)
                    this.Updated(this, EventArgs.Empty);
            }
            return b;
        }

    }
}
