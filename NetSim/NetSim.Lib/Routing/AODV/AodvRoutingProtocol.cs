using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvRoutingProtocol : NetSimRoutingProtocol
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AodvRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public AodvRoutingProtocol(NetSimClient client) : base(client) { }

        #endregion

        #region Properties



        #endregion

        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            //intialize routing table
            this.Table = new AodvTable();
        }

        public override void PerformRoutingStep()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendMessage(NetSimMessage message)
        {
            string nextHopId = GetRoute(message.Receiver);

            //"hack" to determine the receiver endpoint of message
            message.NextReceiver = nextHopId;

            Client.Connections[nextHopId].StartTransportMessage(message);
        }

    
    }
}
