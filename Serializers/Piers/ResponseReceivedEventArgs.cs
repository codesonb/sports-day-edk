using System;

namespace Data.Piers
{
    public class ResponseReceivedEventArgs : EventArgs
    {
        public ResponseReceivedEventArgs(IResponse rsp) { this.Response = rsp; }
        public IResponse Response { get; }
        public bool Cancel { get; set; }
    }
}
