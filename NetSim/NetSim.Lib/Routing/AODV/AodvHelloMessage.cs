﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvHelloMessage : NetSimMessage
    {
        public override object Clone()
        {
            return new AodvHelloMessage() { Receiver = this.Receiver, Sender = this.Sender };
        }
    }
}
