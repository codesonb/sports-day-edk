
namespace EDKv5.SchedulerService
{
    public class AssemblePeriod : Period
    {

        public AssemblePeriod(double duration, string name)
            : base(duration)
        {
            Name = name;
        }

        public override Cell[] Cells
        {
            get
            {
                return new Cell[] { new Cell(Group.Team, Name) };
            }
        }

        public string Name { get; set; }

    }
}
