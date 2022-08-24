using Data.Piers;
using EDKv5.Protocols;
using System;

namespace EDKv5.MonitorServices
{
    public delegate void ResponseEventHandler<T>(T responseData) where T : IResponse;

    // Notify <Mediator>
    // http://www.marco.panizza.name/dispenseTM/slides/exerc/eventNotifier/eventNotifier.html
    public class ResponseMediator
    {
        public event EventHandler<FailureEventArgs> Failed;
        public event ResponseEventHandler<EventOutlinesResponse> GotEventOutlines;
        public event ResponseEventHandler<CompetitionResponse> GotCompetitionDetail;

        /* singleton pattern */
        private ResponseMediator() { }
        private static ResponseMediator _ins;

        public static ResponseMediator Instance
        {
            get
            {
                if (null == _ins) _ins = new ResponseMediator();
                return _ins;
            }
        }
        /* singleton pattern */

        internal void onFailure(ICommand originalEvent, string message)
        {
            if (null != this.Failed)
                this.Failed(originalEvent, new FailureEventArgs(message));
        }
        internal void onGotEventOutlines(EventOutlinesResponse response)
        {
            if (null != this.GotEventOutlines)
                this.GotEventOutlines(response);
        }
        internal void onGotCompetitionDetail(CompetitionResponse response)
        {
            if (null != this.GotCompetitionDetail)
                this.GotCompetitionDetail(response);
        }

    }
}
