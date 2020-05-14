using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperWebSocket
{
    public class WebSocketSession
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Datetime { get; set; }
        public virtual double AliveDateTimes { get; }
    }

    public class WebSocketClientSession : WebSocketSession
    {
        public string Origin { get; set; }
        public string ServerId { get; set; }
        public string ServerName { get; set; }
        public string Fun { get; set; }

        public override double AliveDateTimes {
            get
            {
                return (DateTime.Now - this.Datetime).TotalMilliseconds;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class WebSocketServerSession : WebSocketSession
    {
        public override double AliveDateTimes
        {
            get
            {
                return (DateTime.Now - this.Datetime).TotalMilliseconds;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
