using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimMessage : NetSimItem
    {
        public string Sender { get; set; }

        public string Receiver { get; set; }
    }
}