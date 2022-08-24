using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Launcher
{
    class EventMessage
    {
        public string Time { get; private set; }
        public string Type { get; private set; }
        public string Message { get; private set; }
        public EventMessage(string type, string message)
        {
            this.Time = DateTime.Now.ToString("hh:mm tt");
            this.Type = type;
            this.Message = message;
        }
    }
}
