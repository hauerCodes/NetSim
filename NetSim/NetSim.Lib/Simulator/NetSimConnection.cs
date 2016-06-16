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
            this.PendingMessages = new Queue<NetSimMessage>();
        }

        public Queue<NetSimMessage> PendingMessages { get; set; }

        public NetSimClient EndPointA { get; set; }

        public NetSimClient EndPointB { get; set; }

        public int Metric { get; set; } = 1;

        public bool IsOffline { get; set; } = false;

        public bool IsTransmitting => PendingMessages.Count > 0;

        public void StartTransportMessage(NetSimMessage message)
        {
            if (EndPointA == null || EndPointB == null || IsOffline) return;

            if (EndPointA.Id.Equals(message.Receiver) || EndPointB.Id.Equals(message.Receiver))
            {
                PendingMessages.Enqueue(message);
            }
        }

        public void EndTransportMessages()
        {
            while(PendingMessages.Count > 0 )
            {
                var message = PendingMessages.Dequeue();

                if (EndPointA != null && message.Receiver.Equals(EndPointA.Id))
                {
                    EndPointA.ReceiveMessage(message);
                }

                if (EndPointB != null && message.Receiver.Equals(EndPointB.Id))
                {
                    EndPointB.ReceiveMessage(message);
                }
            }
        }

    }
}