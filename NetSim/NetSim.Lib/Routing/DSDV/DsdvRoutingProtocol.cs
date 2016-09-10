// -----------------------------------------------------------------------
// <copyright file="DsdvRoutingProtocol.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsdvRoutingProtocol.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSDV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsdv routing protocol implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimRoutingProtocol" />
    public class DsdvRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// Gets a value indicating whether this instance is initial broadcast.
        /// </summary>
        private bool isInitialBroadcastSend;

        /// <summary>
        /// The offline links
        /// </summary>
        private List<string> offlineLinks;

        /// <summary>
        /// The periodic update counter
        /// </summary>
        private int periodicUpdateCounter = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsdvRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public DsdvRoutingProtocol(NetSimClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Gets or sets the current sequence.
        /// </summary>
        /// <value>
        /// The current sequence.
        /// </value>
        public DsdvSequence CurrentSequence { get; set; }

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
            this.CurrentSequence = new DsdvSequence(this.Client.Id, 0);

            // self routing entry with metric 0 and initial sequence nr
            localTableRef.AddRouteEntry(this.Client.Id, this.Client.Id, 0, this.CurrentSequence);
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // check for topology changes - e.g. check if offline routes are marked as not reachable in routes table
            var topologyChangeUpdate = this.CheckForTopologyUpdates();

            // handle incomming messages
            topologyChangeUpdate = this.HandleIncomingMessages(topologyChangeUpdate);

            // handle offline connections / back online connections
            if (topologyChangeUpdate)
            {
                this.UpdateRoutesForOfflineConnections();
                this.UpdateRoutesForDeletedConnections();
            }

            // if update needed or periocid update
            if (topologyChangeUpdate || this.StepCounter % this.periodicUpdateCounter == 0)
            {
                // if current node is connected to other clients
                if (this.Client.Connections.Count > 0)
                {
                    // increase local sequencenr after first/inital broadcast (stepcounter = 0)
                    if (this.isInitialBroadcastSend && topologyChangeUpdate)
                    {
                        this.CurrentSequence.SequenceNr += 2;
                    }
                    else
                    {
                        // the first broadcast should be sended with sequence A-000
                        this.isInitialBroadcastSend = true;
                    }

                    // send update 
                    this.Client.BroadcastMessage(
                        new DsdvUpdateMessage() { Sender = this.Client.Id, UpdateTable = (DsdvTable)this.Table.Clone() });
                }
            }

            this.StepCounter++;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendMessage(NetSimMessage message)
        {
            string nextHopId = this.GetRoute(message.Receiver);

            if (this.IsConnectionReachable(nextHopId))
            {
                this.Client.Connections[nextHopId].StartTransportMessage(message, this.Client.Id, nextHopId);
            }
            else
            {
                // TODO handle not reachable connection
            }
        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns>The string representation of the routing data.</returns>
        protected override string GetRoutingData()
        {
            return this.Table.ToString();
        }

        /// <summary>
        /// Checks for topology updates.
        /// </summary>
        /// <returns>true if the topology updates have been recognizes; otherwise false.</returns>
        private bool CheckForTopologyUpdates()
        {
            bool topologyChangeUpdate = false;

            // check if all routes for offline connections are marked as not reachable
            foreach (
                var connection in
                this.Client.Connections.Where(c => c.Value.IsOffline && !this.offlineLinks.Contains(c.Key)))
            {
                // connection.key is the "to" destination
                var routeEntry = this.Table.GetRouteFor(connection.Key);

                // if route is reachable - broken link detected
                if (routeEntry != null && routeEntry.IsReachable)
                {
                    // routeEntry.Metric = NotReachable;

                    // change in topology detected
                    topologyChangeUpdate = true;
                }
            }

            // check offline links
            foreach (var connection in this.offlineLinks)
            {
                // if the connection is not offline
                if (!this.Client.Connections[connection].IsOffline)
                {
                    // change in topology detected
                    topologyChangeUpdate = true;
                }
            }

            // check if a direct connections exists which has no route entry -> new connection
            if (this.Client.Connections.Any(c => this.Table.GetRouteFor(c.Key) == null))
            {
                topologyChangeUpdate = true;
            }

            // check if a direct connections doesn't exist which has an direct route entry -> deleted connection
            if (
                this.Table.Entries.Where(e => e.Destination.Equals(e.NextHop) && !e.Destination.Equals(this.Client.Id))
                    .Any(e => !this.Client.Connections.ContainsKey(e.Destination)))
            {
                topologyChangeUpdate = true;
            }

            return topologyChangeUpdate;
        }

        /// <summary>
        /// Handles the incoming messages.
        /// </summary>
        /// <param name="topologyChangeUpdate">The current topology change update state.</param>
        /// <returns>true if a topology change has been recognized; otherwise false</returns>
        private bool HandleIncomingMessages(bool topologyChangeUpdate)
        {
            if (this.Client.InputQueue.Count > 0)
            {
                while (this.Client.InputQueue.Count > 0)
                {
                    var message = this.Client.InputQueue.Dequeue();

                    // if message is update message
                    if (message is DsdvUpdateMessage)
                    {
                        // local client table
                        var dsdvTable = this.Table as DsdvTable;

                        // ReSharper disable once InvertIf
                        if (dsdvTable != null)
                        {
                            // let the local table handle the update message 
                            if (dsdvTable.HandleUpdate(message.Sender, (message as DsdvUpdateMessage).UpdateTable))
                            {
                                // if somethings updated in the table - a topology change was dected 
                                topologyChangeUpdate = true;
                            }
                        }
                    }
                    else
                    {
                        // forward message if client is not reciever
                        if (!message.Receiver.Equals(this.Client.Id))
                        {
                            this.SendMessage(message);
                        }
                        else
                        {
                            this.Client.ReceiveData(message);
                        }
                    }
                }
            }

            return topologyChangeUpdate;
        }

        /// <summary>
        /// Updates the routes for deleted connections.
        /// </summary>
        private void UpdateRoutesForDeletedConnections()
        {
            // foreach direct connected route 
            foreach (
                var routeEntry in
                this.Table.Entries.Where(e => e.Destination.Equals(e.NextHop) && e.Destination != this.Client.Id))
            {
                // if the route does exists - contine to next route entry
                if (this.Client.Connections.ContainsKey(routeEntry.Destination))
                {
                    continue;
                }

                // if the route does'nt exist
                var dsdvTableEntry = routeEntry as DsdvTableEntry;
                if (dsdvTableEntry == null)
                {
                    continue;
                }

                // NOTE: point where not the destination changes a sequence number
                dsdvTableEntry.SequenceNr.SequenceNr += 1;
                (this.Table as DsdvTable)?.SetAllRoutesNotReachableForDisconnectedNextHop(dsdvTableEntry.NextHop);
            }
        }

        /// <summary>
        /// Handles the offline connections.
        /// </summary>
        private void UpdateRoutesForOfflineConnections()
        {
            foreach (var connection in this.Client.Connections.Where(c => c.Value.IsOffline))
            {
                if (this.offlineLinks.Contains(connection.Key))
                {
                    continue;
                }

                // connection.key is the "to" destination
                var routeEntry = this.Table.GetRouteFor(connection.Key);

                // add to offline links
                this.offlineLinks.Add(connection.Key);

                // update metric to not reachable
                // ReSharper disable once ArrangeStaticMemberQualifier
                routeEntry.Metric = DsdvRoutingProtocol.NotReachable;

                // NOTE: point where not the destination changes a sequence number
                var dsdvTableEntry = routeEntry as DsdvTableEntry;
                if (dsdvTableEntry != null)
                {
                    dsdvTableEntry.SequenceNr.SequenceNr += 1;
                    (this.Table as DsdvTable)?.SetAllRoutesNotReachableForDisconnectedNextHop(dsdvTableEntry.NextHop);
                }
            }

            // remove connections from offline links if connection is back online
            foreach (var connection in this.offlineLinks.ToList())
            {
                if (!this.Client.Connections[connection].IsOffline)
                {
                    this.offlineLinks.Remove(connection);
                }
            }
        }
    }
}