using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSDV
{
    public class DsdvRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// The periodic update counter
        /// </summary>
        private int periodicUpdateCounter = 10;

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="DsdvRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public DsdvRoutingProtocol(NetSimClient client) : base(client){}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current sequence.
        /// </summary>
        /// <value>
        /// The current sequence.
        /// </value>
        public DsdvSequence CurrentSequence { get; set; }

        #endregion

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // create table
            this.Table = new DsdvTable();

            // cast to right type
            var localTableRef = (DsdvTable)this.Table;

            // create current (initial) sequence nr (ID-000)
            this.CurrentSequence = new DsdvSequence() { SequenceId = this.Client.Id, SequenceNr = 0 };

            // self routing entry with metric 0 and initial sequence nr
            localTableRef.AddInitialRouteEntry(Client.Id, Client.Id, 0, CurrentSequence);

            //add intial routes for each direct connection
            //foreach(var to in Client.Connections.Keys)
            //{
            //    //localTableRef.AddInitialRouteEntry(to, to, Client.Connections[to].Metric);
            //    localTableRef.AddInitialRouteEntry(to, to, 1);
            //}
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // check for changes - check if offline routes are marked as not reachable in routes table
            var topologyChangeUpdate = CheckForTopologyUpdates();

            // handle incomming messages (only one per step)
            topologyChangeUpdate = HandleIncommingMessages(topologyChangeUpdate);

            // if update needed or periocid update
            if (topologyChangeUpdate || stepCounter % periodicUpdateCounter == 0 )
            {
                // send  update
                Client.BroadcastMessage(new DsdvUpdateMessage()
                {
                    Sender = Client.Id,
                    UpdateTable = (DsdvTable)this.Table.Clone()
                });
            }

            stepCounter++;
        }

        /// <summary>
        /// Handles the incomming messages.
        /// </summary>
        /// <param name="topologyChangeUpdate">if set to <c>true</c> [topology change update].</param>
        /// <returns></returns>
        private bool HandleIncommingMessages(bool topologyChangeUpdate)
        {
            if (Client.InputQueue.Count > 0)
            {
                var message = Client.InputQueue.Dequeue();

                // if message is update message
                if (message is DsdvUpdateMessage)
                {
                    // client table
                    var dsdvTable = Table as DsdvTable;

                    if (dsdvTable != null)
                    {
                        if (dsdvTable.HandleUpdate(message.Sender, (message as DsdvUpdateMessage).UpdateTable))
                        {
                            topologyChangeUpdate = true;
                        }
                    }
                }
                else
                {
                    // forward message if client is not reciever
                    if (!message.Receiver.Equals(this.Client.Id))
                    {
                        this.Client.SendMessage(message);
                    }
                }
            }

            return topologyChangeUpdate;
        }
    }
}
