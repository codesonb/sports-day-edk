using System;

namespace Data.Piers
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public CommandReceivedEventArgs(ICommand cmd) { this.Command = cmd; }
        public ICommand Command { get; }
        public bool Cancel { get; set; }
    }
}
