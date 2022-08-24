
namespace EDKv5.SchedulerService
{

    public abstract class Period : IPeriod
    {
        /// <param name="duration">Duration in minutes</param>
        public Period(double duration)
        {
            Length = duration;
        }

        public abstract Cell[] Cells { get; }
        public double Length { get; private set; }


    }
    
}
