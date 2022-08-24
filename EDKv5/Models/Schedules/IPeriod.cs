namespace EDKv5.SchedulerService
{
    public interface IPeriod
    {
        double Length { get; }
        Cell[] Cells { get; }
    }
    public interface ICompetingPeriod : IPeriod
    {
        Group Add(CompetingGroup competingGroup);
        bool Remove(CompetingGroup competingGroup);
        void Clear();
    }
    public sealed class Cell
    {
        public Cell(Group group, object data) { this.Group = group; this.Data = data; }
        public readonly Group Group;
        public readonly object Data;
    }

    internal static class IPeriodExtension
    {
        public static System.Collections.IEnumerable GetCompetingPeriods(this IScheduleDay day)
        {
            foreach (IPeriod period in day.Periods)
            {
                ICompetingPeriod icp = period as ICompetingPeriod;
                if (null != icp) yield return icp;
            }
        }
    }

}
