using Data.Piers;
using EDKv5.MonitorServices;

namespace EDKv5.Protocols
{
    public class EventOutlinesResponse : IResponse
    {
        public EventOutlinesResponse(EventOutline[] outlines)
        {
            EventOutlines = outlines;
        }

        public EventOutline[] EventOutlines { get; private set; }

        public void Process()
        {
            ResponseMediator rM = ResponseMediator.Instance;
            rM.onGotEventOutlines(this);
        }
    }
}
