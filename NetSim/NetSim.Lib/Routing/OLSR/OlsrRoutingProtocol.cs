// -----------------------------------------------------------------------
// <copyright file="OlsrRoutingProtocol.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrRoutingProtocol.cs</summary>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local
namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Routing.Helpers;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The olsr routing protocol implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimRoutingProtocol" />
    public class OlsrRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// The message handler resolver instance.
        /// </summary>
        private readonly MessageHandlerResolver handlerResolver;

        /// <summary>
        /// The first calculation broadcast flag.
        /// </summary>
        private bool isMprCalculationBroadcoasted;

        /// <summary>
        /// The periodic update counter
        /// </summary>
        private int periodicHelloUpdateCounter = 20;

        /// <summary>
        /// The periodic topology control counter
        /// </summary>
        private int periodicTopologyControlCounter = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public OlsrRoutingProtocol(NetSimClient client)
            : base(client)
        {
            this.handlerResolver = new MessageHandlerResolver(this.GetType());
        }

        /// <summary>
        /// Gets or sets the current sequence.
        /// </summary>
        /// <value>
        /// The current sequence.
        /// </value>
        public OlsrSequence CurrentSequence { get; set; }

        /// <summary>
        /// Gets or sets the multi point relay selector set.
        /// </summary>
        /// <value>
        /// The multi point relay selector set.
        /// </value>
        public List<string> MultiPointRelaySelectorSet { get; set; }

        /// <summary>
        /// Gets or sets the one hop neighbor table.
        /// </summary>
        /// <value>
        /// The one hop neighbor table.
        /// </value>
        public OlsrNeighborTable OneHopNeighborTable { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public OlsrState State { get; set; }

        /// <summary>
        /// Gets or sets the topology table.
        /// </summary>
        /// <value>
        /// The topology table.
        /// </value>
        public OlsrTopologyTable TopologyTable { get; set; }

        /// <summary>
        /// Gets or sets the two hop neighbor table.
        /// </summary>
        /// <value>
        /// The two hop neighbor table.
        /// </value>
        public OlsrNeighborTable TwoHopNeighborTable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether hello update has been received or not
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is hello update; otherwise, <c>false</c>.
        /// </value>
        private bool IsHelloUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is topology update.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is topology update; otherwise, <c>false</c>.
        /// </value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool IsTopologyUpdate { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            // intialize local neighbor tables
            this.OneHopNeighborTable = new OlsrNeighborTable();
            this.TwoHopNeighborTable = new OlsrNeighborTable();
            this.TopologyTable = new OlsrTopologyTable();

            // initialize the multipoint realy selector set
            // this.MultiPointRelaySelectorSet = new Dictionary<string, OLSR.OlsrSequence>();
            this.MultiPointRelaySelectorSet = new List<string>();

            // create current (initial) sequence nr (ID-000)
            this.CurrentSequence = new OlsrSequence(this.Client.Id, 0);

            // set first mpr calcualtion broadcast done to false
            this.isMprCalculationBroadcoasted = false;

            // set protocol state 
            this.State = OlsrState.Hello;
            this.IsHelloUpdate = false;
        }

        /// <summary>
        /// Determines whether the node is MPR neighbor.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the node is a MPR neighbor; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMprNeighbor(string id)
        {
            var entry = this.OneHopNeighborTable.GetEntryFor(id);

            return entry != null && entry.IsMultiPointRelay;
        }

        /// <summary>
        /// Determines whether [is one hop neighbor] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if id is one hop neighbor; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOneHopNeighbor(string id)
        {
            return this.OneHopNeighborTable.GetEntryFor(id) != null;
        }

        /// <summary>
        /// Determines whether [is two hop neighbor] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if id is two hop neighbor; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTwoHopNeighbor(string id)
        {
            return this.TwoHopNeighborTable.GetEntryFor(id) != null;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // reset hello update flag
            this.IsHelloUpdate = false;

            // reset topology updated flag
            this.IsTopologyUpdate = false;

            // TODO check for topology changes - if one hop neighbor is offline or delted
            // if(CheckForTopologyChange())
            // {
            // // update the one hop neighbor table
            // // remove all routes reached trough not reachable connection
            // HandleTopologyChange();
            // // set the mode to broeadcast hello
            // State = OlsrState.Hello;
            // }
            switch (this.State)
            {
                case OlsrState.Hello:

                    // send hello message to all direct links
                    this.BroadcastHelloMessages();

                    this.State = OlsrState.ReceiveHello;
                    break;

                case OlsrState.ReceiveHello:

                    // wait for incoming hello messages 
                    this.HandleIncomingMessages();

                    // if a update of the one or two hop neighbor tables occured - broadcast hello again
                    this.State = this.IsHelloUpdate ? OlsrState.Hello : OlsrState.Calculate;

                    break;

                case OlsrState.Calculate:

                    // calculate mpr set 
                    var mprs = this.CalculateMultiPointRelays();

                    // set one hop neighbors to selected multi point relays
                    this.OneHopNeighborTable.Entries.ForEach(
                        (e) => { e.IsMultiPointRelay = mprs.Contains(e.NeighborId); });

                    // if first broadcast not done
                    if (!this.isMprCalculationBroadcoasted)
                    {
                        this.isMprCalculationBroadcoasted = true;

                        // after calculating multipoint relay set broadcast hello message
                        // including mpr selection info - to  enable other nodes to populate the mpr selector set
                        this.BroadcastHelloMessages();

                        // TODO set mode to wait for hello receive or hello
                        this.State = OlsrState.Hello;
                    }
                    else
                    {
                        // TODO reset mpr broadcasted state 
                        this.isMprCalculationBroadcoasted = false;

                        // this node is a mpr node because is was selected as mpr from other nodes
                        if (this.MultiPointRelaySelectorSet.Count > 0)
                        {
                            // start sharing topology control messages
                            this.State = OlsrState.TopologyControl;

                            // broadcast topology control messages to mpr selector neighbors
                            this.BroadcastTopologyControlMessages();
                        }
                        else
                        {
                            // set protocol to default mode 
                            this.State = OlsrState.HandleIncoming;
                        }
                    }

                    break;

                case OlsrState.TopologyControl:

                    // handle all incoming messages 
                    this.HandleIncomingMessages();

                    // if hello one hop neighbor table was updated - broadcast changes in hello mode
                    if (this.IsHelloUpdate)
                    {
                        // fallback to hello steps
                        this.State = OlsrState.Hello;
                    }

                    if (this.StepCounter % this.periodicTopologyControlCounter == 0)
                    {
                        // broadcast tc messages to mpr selector neighbors
                        this.BroadcastTopologyControlMessages();
                    }

                    // if (IsTopologyUpdate)
                    // {
                    // try to calculate the routing table
                    this.Table = this.CalculateRoutingTable();

                    // }

                    // if periodic time reached - restart hello process
                    if (this.StepCounter % this.periodicHelloUpdateCounter == 0)
                    {
                        this.State = OlsrState.Hello;
                    }

                    break;

                case OlsrState.HandleIncoming:
                    this.HandleIncomingMessages();

                    if (this.IsHelloUpdate)
                    {
                        this.State = OlsrState.Hello;
                    }

                    // try to calculate the routing table
                    this.Table = this.CalculateRoutingTable();

                    // if periodic time - restart hello process
                    if (this.StepCounter % this.periodicHelloUpdateCounter == 0)
                    {
                        this.State = OlsrState.Hello;
                    }

                    break;
            }

            this.StepCounter++;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendMessage(NetSimMessage message)
        {
            if (this.Table == null)
            {
                return;
            }

            string nextHopId = this.GetRoute(message.Receiver);

            if (this.IsConnectionReachable(nextHopId))
            {
                this.Client.Connections[nextHopId].StartTransportMessage(message, this.Client.Id, nextHopId);
            }
        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns>
        /// The string representation of the protocol specific routing data.
        /// </returns>
        protected override string GetRoutingData()
        {
            StringBuilder builder = new StringBuilder();

            if (this.Table != null)
            {
                builder.AppendLine(this.Table.ToString());
                builder.AppendLine();
            }

            builder.AppendFormat("OSLR State: {0:g}\n\n", this.State);

            builder.AppendFormat("One-Hop Neighbor\nId Type\n{0}\n", this.OneHopNeighborTable);

            builder.AppendFormat("Two-Hop Neighbor\nId Trough\n{0}\n", this.TwoHopNeighborTable);

            builder.AppendFormat("MPR Selector Set\n{0}\n\n", string.Join(",", this.MultiPointRelaySelectorSet));

            builder.AppendFormat("Topology Table\n{0}\n", this.TopologyTable);

            return builder.ToString();
        }

        /// <summary>
        /// Broadcasts the hello messages.
        /// </summary>
        private void BroadcastHelloMessages()
        {
            // send hello message with all direct neighbors of this client
            this.Client.BroadcastMessage(
                new OlsrHelloMessage()
                {
                    Neighbors = this.OneHopNeighborTable.Entries.Select(e => e.NeighborId).ToList(),
                    MultiPointRelays =
                        this.OneHopNeighborTable.Entries.Where(e => e.IsMultiPointRelay)
                            .Select(e => e.NeighborId)
                            .ToList()
                });
        }

        /// <summary>
        /// Broadcasts the topology control messages.
        /// </summary>
        private void BroadcastTopologyControlMessages()
        {
            // broadcast topology control messages to mpr selector set neighbors
            foreach (
                var entry in
                this.OneHopNeighborTable.Entries.Where(e => this.MultiPointRelaySelectorSet.Contains(e.NeighborId)))
            {
                // A node P sends control messages only to MPRsel(P)
                var message = new OlsrTopologyControlMessage()
                {
                    Sender = this.Client.Id,
                    Receiver = entry.NeighborId,
                    MultiPointRelaySelectorSet = new List<string>(this.MultiPointRelaySelectorSet)
                };

                if (this.IsConnectionReachable(entry.NeighborId))
                {
                    this.Client.Connections[entry.NeighborId].StartTransportMessage(
                        message,
                        this.Client.Id,
                        entry.NeighborId);
                }
            }
        }

        /// <summary>
        /// Broadcasts the topology control messages.
        /// </summary>
        /// <param name="message">The message.</param>
        private void BroadcastTopologyControlMessages(OlsrTopologyControlMessage message)
        {
            // TODO Check if send to every node or only to MPRsel set

            // broadcast topology control messages to mpr selector set neighbors
            foreach (var entry in this.OneHopNeighborTable.Entries)
            {
                // TODO  .Where(e => MultiPointRelaySelectorSet.Contains(e.NeighborId))
                // copy message before forwarding
                var localMessageCopy = (OlsrTopologyControlMessage)message.Clone();

                // A node P sends control messages only to MPRsel(P)
                localMessageCopy.Receiver = entry.NeighborId;

                if (this.IsConnectionReachable(entry.NeighborId))
                {
                    this.Client.Connections[entry.NeighborId].StartTransportMessage(
                        localMessageCopy,
                        this.Client.Id,
                        entry.NeighborId);
                }
            }
        }

        /// <summary>
        /// Calculates the multi point relays.
        /// </summary>
        /// <returns>The calculated list of multi point relays.</returns>
        private List<string> CalculateMultiPointRelays()
        {
            int stopCounter = 5;
            List<string> coveredTwoHopList = new List<string>();
            List<string> mprList = new List<string>();

            // List<string> notCoveredTwoHopList = new List<string>();

            // step 1 - add every entry from one hop neighbors that has only one edge to two hop neighbors
            mprList.AddRange(
                this.TwoHopNeighborTable.Entries.Where(e => e.AccessibleThrough.Count == 1)
                    .Select(e => e.AccessibleThrough[0]));

            // create are two hop neighbor covered list
            foreach (var mpr in mprList)
            {
                coveredTwoHopList.AddRange(
                    this.TwoHopNeighborTable.Entries.Where(e => e.AccessibleThrough.Contains(mpr))
                        .Where(e => !coveredTwoHopList.Contains(e.NeighborId))
                        .Select(e => e.NeighborId));
            }

            if (coveredTwoHopList.Count == this.TwoHopNeighborTable.Entries.Count)
            {
                return mprList;
            }

            // step 2 add one hop neighbor that covers most remaining two hop neighbors
            while (coveredTwoHopList.Count != this.TwoHopNeighborTable.Entries.Count || stopCounter-- > 0)
            {
                List<string> notCoveredOneHopList =
                    this.OneHopNeighborTable.Entries.Where(e => !mprList.Contains(e.NeighborId))
                        .Select(e => e.NeighborId)
                        .Distinct()
                        .ToList();

                // notCoveredTwoHopList.AddRange(
                // OneHopNeighborTable.Entries.Where(e => !coveredTwoHopList.Contains(e.NeighborId)).Select(e => e.NeighborId));
                Dictionary<string, int> coverage = new Dictionary<string, int>();

                foreach (var onehopEntry in
                    this.OneHopNeighborTable.Entries.Where(e => notCoveredOneHopList.Contains(e.NeighborId)))
                {
                    coverage[onehopEntry.NeighborId] =
                        this.TwoHopNeighborTable.Entries.Count(
                            e => e.AccessibleThrough.Contains(onehopEntry.NeighborId));
                }

                mprList.Add(coverage.FirstOrDefault(c => c.Value.Equals(coverage.Max(m => m.Value))).Key);

                coveredTwoHopList.Clear();

                foreach (var mpr in mprList)
                {
                    coveredTwoHopList.AddRange(
                        this.TwoHopNeighborTable.Entries.Where(e => e.AccessibleThrough.Contains(mpr))
                            .Where(e => !coveredTwoHopList.Contains(e.NeighborId))
                            .Select(e => e.NeighborId));
                }
            }

            return mprList;
        }

        /// <summary>
        /// Calculates the routing table.
        /// </summary>
        /// <returns>The created routing table.</returns>
        private OlsrTable CalculateRoutingTable()
        {
            int stopCounter = 100;

            OlsrTable localTable = new OlsrTable();
            OlsrTopologyTable localTopoTable = (OlsrTopologyTable)this.TopologyTable.Clone();

            // add local loopback 
            localTable.AddRouteEntry(this.Client.Id, this.Client.Id, 0);

            // add each one hop neighbor entry
            this.OneHopNeighborTable.Entries.ForEach(e => localTable.AddRouteEntry(e.NeighborId, e.NeighborId, 1));

            // set hopcount to 1
            var hopcount = 1;

            // If the topology table is empty, return the ﬁnished routing table
            while (localTopoTable.Entries.Count > 0 && stopCounter-- > 0)
            {
                foreach (var destination in localTable.Entries.Select(e => e.Destination))
                {
                    // For each (T, H, M), delete all( , T) from the topology table
                    localTopoTable.Entries.Where(e => e.MprSelectorId.Equals(destination))
                        .ToList()
                        .ForEach(e => localTopoTable.Entries.Remove(e));
                }

                if (localTopoTable.Entries.Count == 0)
                {
                    return localTable;
                }

                // For each (N, T) in the topology table, look for (N, H, h) in the routing table
                // and - if it exists - insert(T, H, h + 1)
                foreach (var entry in localTopoTable.Entries)
                {
                    var searchedRoute = localTable.GetRouteFor(entry.OriginatorId);

                    if (searchedRoute != null && !localTable.Entries.Any(e => e.Destination.Equals(entry.MprSelectorId)))
                    {
                        localTable.AddRouteEntry(entry.MprSelectorId, searchedRoute.NextHop, hopcount + 1);
                    }
                }

                // Increment hop count h to h + 1 
                hopcount += 1;
            }

            return localTable;
        }

        /// <summary>
        /// Handles the received message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DefaultIncomingMessageHandler(NetSimMessage message)
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

        /// <summary>
        /// Handles the incoming messages.
        /// </summary>
        private void HandleIncomingMessages()
        {
            if (this.Client.InputQueue.Count <= 0)
            {
                return;
            }

            while (this.Client.InputQueue.Count > 0)
            {
                // dequues message to handle
                var message = this.Client.InputQueue.Dequeue();

                // searches a handler method with the dsrmessagehandler attribute and the 
                // right message type and for incoming(false) or outgoing (true) messages.
                // e.g. IncomingDsrRouteRequestMessageHandler
                var method = this.handlerResolver.GetHandlerMethod(message.GetType(), false);

                if (method != null)
                {
                    // call handler
                    method.Invoke(this, new object[] { message });
                }
                else
                {
                    // if method not found - use default method to handle message (e.g. data mesage)
                    this.DefaultIncomingMessageHandler(message);
                }
            }
        }

        /// <summary>
        /// Handles the received hello message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(OlsrHelloMessage), Outgoing = false)]
        private void IncomingOlsrHelloMessageHandler(NetSimMessage message)
        {
            var olsrMessage = (OlsrHelloMessage)message;

            // upate one hop neighbors
            if (this.OneHopNeighborTable.GetEntryFor(message.Sender) == null)
            {
                this.OneHopNeighborTable.AddEntry(message.Sender);
                this.IsHelloUpdate = true;
            }

            if (olsrMessage.Neighbors != null && olsrMessage.Neighbors.Any())
            {
                // update two hop neighbors
                foreach (string twohopneighbor in olsrMessage.Neighbors)
                {
                    // if twohop neighbor is also one hop neighbor ignore entry 
                    if (this.OneHopNeighborTable.GetEntryFor(twohopneighbor) != null)
                    {
                        continue;
                    }

                    // if two hop neighbor is this client itself - ingore entry
                    if (twohopneighbor.Equals(this.Client.Id))
                    {
                        continue;
                    }

                    // search twohop neighbor entry
                    var twoHopBeighbor = this.TwoHopNeighborTable.GetEntryFor(twohopneighbor);

                    // upate two hop neighbors table
                    if (twoHopBeighbor == null)
                    {
                        // if neighbor not exists add it 
                        this.TwoHopNeighborTable.AddEntry(twohopneighbor, message.Sender);
                        this.IsHelloUpdate = true;
                    }
                    else
                    {
                        // if neighbor exists check if sender is listed in accessthrough
                        if (!twoHopBeighbor.AccessibleThrough.Contains(message.Sender))
                        {
                            // if not add it to the accessable through
                            twoHopBeighbor.AccessibleThrough.Add(message.Sender);
                            this.IsHelloUpdate = true;
                        }
                    }
                }
            }

            // if hello messages has info about multipoint relays
            if (olsrMessage.MultiPointRelays != null && olsrMessage.MultiPointRelays.Any())
            {
                // if this client was selected as mpr by sender of this message 
                if (olsrMessage.MultiPointRelays.Contains(this.Client.Id))
                {
                    if (!this.MultiPointRelaySelectorSet.Contains(olsrMessage.Sender))
                    {
                        // add sender to mpr selection set
                        this.MultiPointRelaySelectorSet.Add(olsrMessage.Sender);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the received TopologyControl message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(OlsrTopologyControlMessage), Outgoing = false)]
        private void IncomingOlsrTopologyControlMessageHandler(NetSimMessage message)
        {
            OlsrTopologyControlMessage topMessage = (OlsrTopologyControlMessage)message;

            // if own tc message 
            if (topMessage.Sender.Equals(this.Client.Id))
            {
                // drop message 
                return;
            }

            // process tc message
            foreach (var mprSelector in topMessage.MultiPointRelaySelectorSet)
            {
                if (
                    !this.TopologyTable.Entries.Any(
                        e => e.OriginatorId.Equals(topMessage.Sender) && e.MprSelectorId.Equals(mprSelector)))
                {
                    this.TopologyTable.AddEntry(topMessage.Sender, mprSelector);
                    this.IsTopologyUpdate = true;
                }
            }

            // A node P forwards control messages only from MPRsel(P)
            if (this.MultiPointRelaySelectorSet.Contains(topMessage.Sender))
            {
                // forward message
                this.BroadcastTopologyControlMessages(topMessage);
            }
        }
    }
}