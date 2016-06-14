using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using NetSim.Lib;
using NetSim.Lib.Simulator;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimConnection : NetSimItem
    {
        public NetSimClient From { get; set; }

        public NetSimClient To { get; set; }

        public int Metric { get; set; }

        public void TransportMessage(NetSimMessage message)
        {
            throw new System.NotImplementedException();
        }

    }
}