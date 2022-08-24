using Data.Piers;
using System;

namespace EDKv5.MonitorServices
{
    public class MonitorMediator
    {
        public event EventHandler<FailureEventArgs> Failed;

        /* singleton pattern */
        private MonitorMediator() { }
        private static MonitorMediator _ins;
        
        public static MonitorMediator Instance
        {
            get
            {
                if (null == _ins) _ins = new MonitorMediator();
                return _ins;
            }
        }
        /* singleton pattern */
        //events
        public event EventHandler<EventUpdatedEventArgs> EventCompleted;
        public event EventHandler<CompetitionStateUpdatedEventArgs> CompetitionCompleted;
        public event EventHandler<RecordBreakedEventArgs> RecordBreaked;

        internal void onEventStateUpdated(EventUpdatedEventArgs e)
        {
            if (null != this.EventCompleted)
                EventCompleted(this, e);
        }
        internal void onCompetitionStateUpdated(CompetitionStateUpdatedEventArgs e)
        {
            if (null != this.CompetitionCompleted)
                CompetitionCompleted(this, e);
        }
        internal void onRecordBreaked(RecordBreakedEventArgs e)
        {
            if (null != this.RecordBreaked)
                RecordBreaked(this, e);
        }

        internal void onFailure(ICommand originalEvent, string message)
        {
            if (null != this.Failed)
                this.Failed(originalEvent, new FailureEventArgs(message));
        }
    }
}
