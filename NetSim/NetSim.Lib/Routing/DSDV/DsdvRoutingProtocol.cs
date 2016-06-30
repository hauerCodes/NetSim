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

        /// <summary>
        /// Gets a value indicating whether this instance is initial broadcast.
        /// </summary>
        private bool isInitialBroadcastSend;

        /// <summary>
        /// The offline links
        /// </summary>
        private List<string> offlineLinks;

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="DsdvRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public DsdvRoutingProtocol(NetSimClient client) : base(client) { }

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
            // call base initialization (stepcounter and data)
            base.Initialize();

            // set intial broadcast to false
            this.isInitialBroadcastSend = false;

            // initial offline links stroage
            this.offlineLinks = new List<string>();

            // create table
            this.Table = new DsdvTable();

            // cast to right type
            var localTableRef = (DsdvTable)this.Table;

            // create current (initial) sequence nr (ID-000)
            this.CurrentSequence = new DsdvSequence() { SequenceId = this.Client.Id, SequenceNr = 0 };

            // self routing entry with metric 0 and initial sequence nr
            localTableRef.AddInitialRouteEntry(Client.Id, Client.Id, 0, CurrentSequence);
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // check for topology changes - e.g. check if offline routes are marked as not reachable in routes table
            var topologyChangeUpdate = CheckForTopologyUpdates();

            // handle incomming messages
            topologyChangeUpdate = HandleIncommingMessages(topologyChangeUpdate);

            // handle offline connections / back online connections
            if (topologyChangeUpdate)
            {
                UpdateRoutesForOfflineConnections();
            }

            // if update needed or periocid update
            if (topologyChangeUpdate || stepCounter % periodicUpdateCounter == 0)
            {
                // if current node is connected to other clients
                if (Client.Connections.Count > 0)
                {
                    // increase local sequencenr after first/inital broadcast (stepcounter = 0)
                    if (isInitialBroadcastSend && topologyChangeUpdate)
                    {
                        CurrentSequence.SequenceNr += 2;
                    }
                    else
                    {
                        //the first broadcast should be sended with sequence A-000
                        isInitialBroadcastSend = true;
                    }

                    // send update 
                    Client.BroadcastMessage(new DsdvUpdateMessage()
                    {
                        Sender = Client.Id,
                        UpdateTable = (DsdvTable)this.Table.Clone()
                    });
                }
            }

            //HandleOutgoingMessages();

            stepCounter++;
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

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns></returns>
        protected override string GetRoutingData()
        {
            return Table.ToString();
        }

        /// <summary>
        /// Checks for topology updates.
        /// </summary>
        /// <returns></returns>
        private bool CheckForTopologyUpdates()
        {
            bool topologyChangeUpdate = false;

            // check if alle routees for offline connections are marked as not reachable
            foreach (var connection in Client.Connections.Where(c => c.Value.IsOffline && !offlineLinks.Contains(c.Key)))
            {
                // connection.key is the "to" destination
                var routeEntry = Table.GetRouteFor(connection.Key);

                // if route is reachable - broken link detected
                if (routeEntry != null && routeEntry.IsReachable)
                {
                    //routeEntry.Metric = NotReachable;
                    topologyChangeUpdate = true;
                }
            }

            // check offline links
            foreach (var connection in offlineLinks)
            {
                if (!Client.Connections[connection].IsOffline)
                {
                    topologyChangeUpdate = true;
                }
            }

            ////check all routes marked as not reachable - update if connection is no longer offline
            //foreach (var route in Table.Entries.Where(e => e.Metric == NotReachable))
            //{
            //    // if route is not a direct connection
            //    if (!Client.Connections.ContainsKey(route.Destination))
            //    {
            //        continue;
            //    }

            //    // if connection is not offline update the metric of route - bring route/connection back online
            //    if (!Client.Connections[route.Destination].IsOffline)
            //    {
            //        route.Metric = 1;
            //        topologyChangeUpdate = true;
            //    }
            //}

            // check if a direct connections exists which has no route entry
            if (Client.Connections.Any(c => Table.GetRouteFor(c.Key) == null))
            {
                topologyChangeUpdate = true;
            }

            return topologyChangeUpdate;
        }

        /// <summary>
        /// Handles the offline connections.
        /// </summary>
        private void UpdateRoutesForOfflineConnections()
        {
            foreach (var connection in Client.Connections.Where(c => c.Value.IsOffline))
            {
                if (offlineLinks.Contains(connection.Key)) continue;

                // connection.key is the "to" destination
                var routeEntry = Table.GetRouteFor(connection.Key);

                // add to offline links
                offlineLinks.Add(connection.Key);

                // update metric to not reachable
                routeEntry.Metric = NotReachable;

                // NOTE: only point where not the destination changes a sequence number
                var dsdvTableEntry = routeEntry as DsdvTableEntry;
                if (dsdvTableEntry != null)
                {
                    dsdvTableEntry.SequenceNr.SequenceNr += 1;
                }
            }

            // remove connections from offline links if connection is back online
            foreach (var connection in offlineLinks.ToList())
            {
                if (!Client.Connections[connection].IsOffline)
                {
                    offlineLinks.Remove(connection);
                }
            }

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
                while (Client.InputQueue.Count > 0)
                {
                    var message = Client.InputQueue.Dequeue();

                    // if message is update message
                    if (message is DsdvUpdateMessage)
                    {
                        // client table
                        var dsdvTable = Table as DsdvTable;

                        // ReSharper disable once InvertIf
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
                            SendMessage(message);
                        }
                        else
                        {
                            Client.ReceiveData(message);
                        }
                    }
                }
            }

            return topologyChangeUpdate;
        }
    }
}
