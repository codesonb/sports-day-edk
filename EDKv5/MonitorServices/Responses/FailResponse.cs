

using Data.Piers;
using EDKv5.MonitorServices;

namespace EDKv5.Protocols
{
    public class FailResponse : IResponse
    {
        public FailResponse(ICommand originalCommand, string Message)
        {
            this.OriginalCommand = originalCommand;
            this.Message = Message;
        }

        readonly ICommand OriginalCommand;
        readonly string Message;

        public void Process()
        {
            MonitorMediator.Instance.onFailure(OriginalCommand, Message);
        }
    }
}
