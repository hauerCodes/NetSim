using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvRreqMessage : NetSimMessage
    {
        public override object Clone()
        {
            return new AodvRreqMessage() { Receiver = this.Receiver, Sender = this.Sender };
        }
    }
}
