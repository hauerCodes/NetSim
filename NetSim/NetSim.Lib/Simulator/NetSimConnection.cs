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
        public NetSimConnection()
        {
        }

        public NetSimClient From { get; set; }

        public NetSimClient To { get; set; }

        public int Metric { get; set; } = 1;

        public bool IsOffline { get; set; } = false;

        public void TransportMessage(NetSimDestination destination, NetSimMessage message)
        {
            if(From != null && destination == NetSimDestination.From)
            {
                From.ReceiveMessage(message);
            }

            if(To != null && destination == NetSimDestination.To)
            {
                To.ReceiveMessage(message);
            }
        }

    }
}