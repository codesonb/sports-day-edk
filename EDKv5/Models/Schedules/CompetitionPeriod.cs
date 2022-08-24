using System.Collections.Generic;

namespace EDKv5.SchedulerService
{
    public class CompetitionPeriod : Period, ICompetingPeriod
    {
        
        public CompetitionPeriod(double duration)
            : base(duration)
        { }

        Group groupMapTable = Group.None;
        SortedList<Group, CompetingGroup> _cells = new SortedList<Group, CompetingGroup>();

        public override Cell[] Cells
        {
            get
            {
                List<Cell> cs = new List<Cell>();
                foreach (CompetingGroup competingGroup in _cells.Values)
                    cs.Add(new Cell(competingGroup.Group, competingGroup));
                return cs.ToArray();
            }
        }

        public Group Add(CompetingGroup competingGroup)
        {
            Group check = competingGroup.Group & groupMapTable;

            if (check == Group.None)
            {
                groupMapTable |= competingGroup.Group;
                _cells.Add(competingGroup.Group, competingGroup);
            }
            return check;
        }
        public bool Remove(CompetingGroup competingGroup)
        {
            if (_cells[competingGroup.Group] == competingGroup)
            {
                _cells.Remove(competingGroup.Group);
                groupMapTable &= ~competingGroup.Group;
                return true;
            }
            return false;
        }
        public void Clear()
        {
            _cells.Clear();
            groupMapTable = Group.None;
        }
    }
}
