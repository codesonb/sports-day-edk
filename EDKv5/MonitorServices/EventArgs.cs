using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5.MonitorServices
{
    public abstract class MonitorEventArgs : EventArgs { }

    public class FailureEventArgs : EventArgs
    {
        public FailureEventArgs(string message) { this.Message = message; }
        public readonly string Message;
    }
    public class CompetitionStateUpdatedEventArgs : MonitorEventArgs
    {
        public readonly ICompetition Competition;
        public readonly EventCompletionState State;
        public CompetitionStateUpdatedEventArgs(ICompetition competition, EventCompletionState state)
        {
            this.Competition = competition;
            this.State = state;
        }
    }
    public class EventUpdatedEventArgs : MonitorEventArgs
    {
        public readonly Group Group;
        public readonly Event Event;
        public EventUpdatedEventArgs(Group group, Event @event)
        {
            this.Group = group;
            this.Event = @event;
        }
    }
    public class EventGroupCompeletedEventArgs : MonitorEventArgs
    {
        public readonly Event Event;
        public readonly Group Group;
        public EventGroupCompeletedEventArgs(Event @event, Group group)
        {
            this.Event = @event;
            this.Group = group;
        }
    }
    public class RecordBreakedEventArgs : MonitorEventArgs
    {
        public readonly ICompetition Competition;
        public readonly Participant Participant;
        public readonly CompetitionResult Result;
        public Event Event { get { return Competition.Event;  } }
        public Group Group { get { return Competition.Event.Contains(Participant.Group); } }
        public RecordBreakedEventArgs(ICompetition competition, Participant participant, CompetitionResult result)
        {
            this.Competition = competition;
            this.Participant = participant;
            this.Result = result;
        }
    }
}
