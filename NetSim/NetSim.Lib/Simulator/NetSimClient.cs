using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimClient : NetSimItem
    {
        public NetSimClient(string id, NetSimLocation location)
        {
            this.Id = id;
            this.Location = location;
            this.InputQueue = new Queue<NetSimMessage>();
            this.Connections = new Dictionary<string, NetSimConnection>();
        }

        public Dictionary<string, NetSimConnection> Connections { get; set; }

        public Queue<NetSimMessage> InputQueue { get; set; }

        public void ReceiveMessage(NetSimMessage message)
        {
            throw new System.NotImplementedException();
        }

    }
}