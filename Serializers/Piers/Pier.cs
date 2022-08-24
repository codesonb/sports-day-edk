using System;
using System.Net.Sockets;
using System.Threading;

using Data.Serializers;

namespace Data.Piers
{
    public sealed class Pier
    {
        public event EventHandler<CommandReceivedEventArgs> CommandReceived;
        public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;

        public Pier(TcpClient client)
        {
            this.client = client;
            ns = client.GetStream();
        }

        TcpClient client;
        NetworkStream ns;
        Mutex _mutex_send_cmd = new Mutex();

        public bool Receive()
        {
            if (0 == client.Available) return false;
            ISerializer sr = BinarySerializer.GetInstance();
            object dyn = sr.Deserialize(ns);

            ICommand cmd;
            IResponse rsp;

            if (null != (cmd = dyn as ICommand))
            {
                var e = new CommandReceivedEventArgs(cmd);
                onCommandReceived(e);
                if (!e.Cancel)
                {
                    rsp = cmd.Execute();
                    if (null != rsp) { this.Send(rsp); }
                }
            }
            else if (null != (rsp = dyn as IResponse))
            {
                var e = new ResponseReceivedEventArgs(rsp);
                onResponseReceived(e);
                if (!e.Cancel) rsp.Process();
            }
            else
            {
                throw new Exception("Received unexpected object : " + dyn.GetType().Name);
            }
            return true;
        }
        public void Send(ICommand cmd) { send(cmd); }
        public void Send(IResponse rsp) { send(rsp); }
        private void send(object graph)
        {
            if (null == graph) return;
            _mutex_send_cmd.WaitOne();
            ISerializer sr = BinarySerializer.GetInstance();
            sr.Serialize(ns, graph);
            _mutex_send_cmd.ReleaseMutex();
        }

        public void Close()
        {
            client.Close();
        }

        private void onCommandReceived(CommandReceivedEventArgs e)
        {
            if (null != this.CommandReceived)
                CommandReceived(this, e);
        }
        private void onResponseReceived(ResponseReceivedEventArgs e)
        {
            if (null != this.ResponseReceived)
                ResponseReceived(this, e);
        }
    }
}
