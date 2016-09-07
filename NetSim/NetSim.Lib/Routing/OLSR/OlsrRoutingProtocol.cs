using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Routing.Helpers;
using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;
// ReSharper disable UnusedMember.Local

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// The message handler resolver instance.
        /// </summary>
        private readonly MessageHandlerResolver handlerResolver;

        /// <summary>
        /// The periodic update counter
        /// </summary>
        private int periodicHelloUpdateCounter = 20;

        /// <summary>
        /// The periodic toplogy control counter
        /// </summary>
        private int periodicToplogyControlCounter = 10;

        /// <summary>
        /// The first calculation broadcoast flag.
        /// </summary>
        private bool isFirstMprCalculationBroadcoasted;

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public OlsrRoutingProtocol(NetSimClient client) : base(client)
        {
            handlerResolver = new MessageHandlerResolver(this.GetType());
        }

        /// <summary>
        /// Gets the output message queue (should be used only for data messages).
        /// </summary>
        /// <value>
        /// The output queue.
        /// </value>
        //TODO remove if not needed
        public Queue<NetSimMessage> OutputQueue { get; private set; }

        /// <summary>
        /// Gets or sets the one hop neighbor table.
        /// </summary>
        /// <value>
        /// The one hop neighbor table.
        /// </value>
        public OlsrNeighborTable OneHopNeighborTable { get; set; }

        /// <summary>
        /// Gets or sets the two hop neighbor table.
        /// </summary>
        /// <value>
        /// The two hop neighbor table.
        /// </value>
        public OlsrNeighborTable TwoHopNeighborTable { get; set; }

        /// <summary>
        /// Gets or sets the toplogy table.
        /// </summary>
        /// <value>
        /// The toplogy table.
        /// </value>
        public OlsrToplogyTable ToplogyTable { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public OlsrState State { get; set; }

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
        /// The indicator for hello update has been received
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
        private bool IsTopologyUpdate { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            //intialize local neighbor tables
            this.OneHopNeighborTable = new OlsrNeighborTable();
            this.TwoHopNeighborTable = new OlsrNeighborTable();
            this.ToplogyTable = new OlsrToplogyTable();

            //prepare the output message queue
            this.OutputQueue = new Queue<NetSimMessage>();

            // initialize the multipoint realy selector set
            //this.MultiPointRelaySelectorSet = new Dictionary<string, OLSR.OlsrSequence>();
            this.MultiPointRelaySelectorSet = new List<string>();

            // create current (initial) sequence nr (ID-000)
            this.CurrentSequence = new OlsrSequence(this.Client.Id, 0);

            // set first mpr calcualtion broadcast done to false
            this.isFirstMprCalculationBroadcoasted = false;

            //set protocol state 
            this.State = OlsrState.Hello;
            this.IsHelloUpdate = false;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // reset hello update flag
            IsHelloUpdate = false;
            //reset topology updated flag
            IsTopologyUpdate = false;

            // TODO check for topology changes - if one hop neighbor is offline or delted
            //if(CheckForTopologyChange())
            //{
            //    // update the one hop neighbor table
            //    // remove all routes reached trough not reachable connection
            //    HandleTopologyChange();
            //    
            //    // set the mode to broeadcast hello
            //    State = OlsrState.Hello;
            //}

            switch (State)
            {
                case OlsrState.Hello:
                    // send hello message to all direct links
                    BroadcastHelloMessages();

                    State = OlsrState.ReceiveHello;
                    break;

                case OlsrState.ReceiveHello:
                    // wait for incoming hello messages 
                    HandleIncomingMessages();

                    // if a update of the one or two hop neighbor tables occured - broadcast hello again
                    if (IsHelloUpdate)
                    {
                        State = OlsrState.Hello;
                    }
                    else
                    {
                        // otherwise start to calculate mpr set
                        this.State = OlsrState.Calculate;
                    }

                    break;

                case OlsrState.Calculate:
                    // calculate mpr set 
                    var mprs = CalculateMultiPointRelays();

                    // set one hop neighbors to selected multi point relays
                    OneHopNeighborTable.Entries.ForEach((e) =>
                    {
                        e.IsMultiPointRelay = mprs.Contains(e.NeighborId);
                    });

                    // if first broadcast not done
                    if (!isFirstMprCalculationBroadcoasted)
                    {
                        isFirstMprCalculationBroadcoasted = true;

                        // after calculating multipoint relay set broadcast hello message
                        // including mpr selection info - to  enable other nodes to populate the mpr selector set
                        BroadcastHelloMessages();

                        // TODO set mode to wait for hello receive or hello
                        this.State = OlsrState.Hello;
                    }
                    else
                    {

                        // this node is a mpr node because is was selected as mpr from other nodes
                        if (MultiPointRelaySelectorSet.Count > 0)
                        {
                            // start sharing topology control messages
                            this.State = OlsrState.TopologyControl;

                            // broadcast topology control messages to mpr selector neighbors
                            BroadcastTopologyControlMessages();
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
                    HandleIncomingMessages();

                    // if hello one hop neighbor table was updated - broadcast changes in hello mode
                    if (IsHelloUpdate)
                    {
                        //fallback to hello steps
                        State = OlsrState.Hello;
                    }

                    if (stepCounter % periodicToplogyControlCounter == 0)
                    {
                        // broadcast tc messages to mpr selector neighbors
                        BroadcastTopologyControlMessages();
                    }

                    //if (IsTopologyUpdate)
                    //{
                    // try to calculate the routing table
                    this.Table = CalculateRoutingTable();
                    //}

                    // if periodic time reached - restart hello process
                    if (stepCounter % periodicHelloUpdateCounter == 0)
                    {
                        State = OlsrState.Hello;
                    }

                    break;

                case OlsrState.HandleIncoming:
                    HandleIncomingMessages();

                    if (IsHelloUpdate)
                    {
                        State = OlsrState.Hello;
                    }

                    // try to calculate the routing table
                    this.Table = CalculateRoutingTable();

                    // if periodic time - restart hello process
                    if (stepCounter % periodicHelloUpdateCounter == 0)
                    {
                        State = OlsrState.Hello;
                    }

                    break;
            }

            stepCounter++;
        }


        /// <summary>
        /// Calculates the routing table.
        /// </summary>
        /// <returns></returns>
        private OlsrTable CalculateRoutingTable()
        {
            int hopcount = 0;
            int stopCounter = 100;

            OlsrTable localTable = new OlsrTable();
            OlsrToplogyTable localTopoTable = (OlsrToplogyTable)ToplogyTable.Clone();

            //add local loopback 
            localTable.AddRouteEntry(Client.Id, Client.Id, 0);

            //add each one hop neighbor entry
            OneHopNeighborTable.Entries.ForEach(e => localTable.AddRouteEntry(e.NeighborId, e.NeighborId, 1));

            // uddate hopcount
            hopcount = 1;

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
                    var searchedRoute = localTable.GetRouteFor(entry.OrigniatorId);

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
        /// Calculates the multi point relays.
        /// </summary>
        /// <returns></returns>
        private List<string> CalculateMultiPointRelays()
        {
            int stopCounter = 5;
            List<string> coveredTwoHopList = new List<string>();
            List<string> mprList = new List<string>();
            //List<string> notCoveredTwoHopList = new List<string>();

            // step 1 - add every entry from one hop neighbors that has only one edge to two hop neighbors
            mprList.AddRange(
                TwoHopNeighborTable.Entries.Where(e => e.AccessableThrough.Count == 1)
                    .Select(e => e.AccessableThrough[0]));

            // create are two hop neighbor covered list
            foreach (var mpr in mprList)
            {
                coveredTwoHopList.AddRange(TwoHopNeighborTable.Entries
                    .Where(e => e.AccessableThrough.Contains(mpr))
                    .Where(e => !coveredTwoHopList.Contains(e.NeighborId))
                    .Select(e => e.NeighborId));
            }

            if (coveredTwoHopList.Count == TwoHopNeighborTable.Entries.Count)
            {
                return mprList;
            }

            // step 2 add one hop neighbor that covers most remaining two hop neighbors
            while (coveredTwoHopList.Count != TwoHopNeighborTable.Entries.Count || stopCounter-- > 0)
            {
                List<string> notCoveredOneHopList = OneHopNeighborTable.Entries
                    .Where(e => !mprList.Contains(e.NeighborId))
                    .Select(e => e.NeighborId)
                    .Distinct().ToList();

                //notCoveredTwoHopList.AddRange(
                //    OneHopNeighborTable.Entries.Where(e => !coveredTwoHopList.Contains(e.NeighborId)).Select(e => e.NeighborId));

                Dictionary<string, int> coverage = new Dictionary<string, int>();

                foreach (var onehopEntry in OneHopNeighborTable.Entries
                    .Where(e => notCoveredOneHopList.Contains(e.NeighborId)))
                {
                    coverage[onehopEntry.NeighborId] =
                        TwoHopNeighborTable.Entries.Count(e => e.AccessableThrough.Contains(onehopEntry.NeighborId));
                }

                mprList.Add(coverage.FirstOrDefault(c => c.Value.Equals(coverage.Max(m => m.Value))).Key);

                coveredTwoHopList.Clear();

                foreach (var mpr in mprList)
                {
                    coveredTwoHopList.AddRange(
                        TwoHopNeighborTable.Entries
                        .Where(e => e.AccessableThrough.Contains(mpr))
                        .Where(e => !coveredTwoHopList.Contains(e.NeighborId))
                        .Select(e => e.NeighborId));
                }
            }

            return mprList;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendMessage(NetSimMessage message)
        {
            if (Table == null)
            {
                return;
            }

            string nextHopId = GetRoute(message.Receiver);

            if (IsConnectionReachable(nextHopId))
            {
                Client.Connections[nextHopId].StartTransportMessage(message, this.Client.Id, nextHopId);
            }
        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns></returns>
        protected override string GetRoutingData()
        {
            StringBuilder builder = new StringBuilder();

            if (Table != null)
            {
                builder.AppendLine(Table.ToString());
                builder.AppendLine();
            }

            builder.AppendFormat("OSLR State: {0:g}\n\n", State);

            builder.AppendFormat("One-Hop Neighbor\nId Type\n{0}\n", OneHopNeighborTable);

            builder.AppendFormat("Two-Hop Neighbor\nId Trough\n{0}\n", TwoHopNeighborTable);

            builder.AppendFormat("MPR Selector Set\n{0}\n\n", String.Join(",", MultiPointRelaySelectorSet));

            builder.AppendFormat("Topology Table\n{0}\n", ToplogyTable);

            return builder.ToString();
        }

        /// <summary>
        /// Broadcasts the hello messages.
        /// </summary>
        private void BroadcastHelloMessages()
        {
            // send hello message with all direct neighbors of this client
            Client.BroadcastMessage(new OlsrHelloMessage()
            {
                Neighbors = OneHopNeighborTable.Entries.Select(e => e.NeighborId).ToList(),
                MultiPointRelays = OneHopNeighborTable.Entries.Where(e => e.IsMultiPointRelay).Select(e => e.NeighborId).ToList()
            });
        }

        /// <summary>
        /// Broadcasts the topology control messages.
        /// </summary>
        private void BroadcastTopologyControlMessages()
        {
            // broadcast topology control messages to mpr selector set neighbors
            foreach (var entry in OneHopNeighborTable.Entries.Where(e => MultiPointRelaySelectorSet.Contains(e.NeighborId)))
            {
                // A node P sends control messages only to MPRsel(P)
                var message = new OlsrTopologyControlMessage()
                {
                    Sender = this.Client.Id,
                    Receiver = entry.NeighborId,
                    MultiPointRelaySelectorSet = new List<string>(MultiPointRelaySelectorSet)
                };

                if (IsConnectionReachable(entry.NeighborId))
                {
                    Client.Connections[entry.NeighborId].StartTransportMessage(message, this.Client.Id, entry.NeighborId);
                }
            }
        }

        /// <summary>
        /// Broadcasts the topology control messages.
        /// </summary>
        /// <param name="message">The message.</param>
        private void BroadcastTopologyControlMessages(OlsrTopologyControlMessage message)
        {
            //TODO Check if send to every node or only to MPRsel set

            // broadcast topology control messages to mpr selector set neighbors
            foreach (var entry in OneHopNeighborTable.Entries) //TODO  .Where(e => MultiPointRelaySelectorSet.Contains(e.NeighborId))
            {
                //copy message before forwarding
                var localMessageCopy = (OlsrTopologyControlMessage)message.Clone();

                // A node P sends control messages only to MPRsel(P)
                localMessageCopy.Receiver = entry.NeighborId;

                if (IsConnectionReachable(entry.NeighborId))
                {
                    Client.Connections[entry.NeighborId].StartTransportMessage(localMessageCopy, this.Client.Id, entry.NeighborId);
                }
            }
        }

        /// <summary>
        /// Handles the incomming messages.
        /// </summary>
        private void HandleIncomingMessages()
        {
            if (Client.InputQueue.Count <= 0)
            {
                return;
            }

            while (Client.InputQueue.Count > 0)
            {
                // dequues message to handle
                var message = Client.InputQueue.Dequeue();

                // searches a handler method with the dsrmessagehandler attribute and the 
                // right message type and for incoming(false) or outgoing (true) messages.
                // e.g. IncomingDsrRouteRequestMessageHandler
                var method = handlerResolver.GetHandlerMethod(message.GetType(), false);

                if (method != null)
                {
                    //call handler
                    method.Invoke(this, new object[] { message });
                }
                else
                {
                    // if method not found - use default method to handle message (e.g. data mesage)
                    DefaultIncomingMessageHandler(message);
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

            //upate one hop neighbors
            if (OneHopNeighborTable.GetEntryFor(message.Sender) == null)
            {
                OneHopNeighborTable.AddEntry(message.Sender);
                IsHelloUpdate = true;
            }

            if (olsrMessage.Neighbors != null && olsrMessage.Neighbors.Any())
            {
                //update two hop neighbors
                foreach (string twohopneighbor in olsrMessage.Neighbors)
                {
                    //if twohop neighbor is also one hop neighbor ignore entry 
                    if (OneHopNeighborTable.GetEntryFor(twohopneighbor) != null)
                    {
                        continue;
                    }

                    // if two hop neighbor is this client itself - ingore entry
                    if (twohopneighbor.Equals(this.Client.Id))
                    {
                        continue;
                    }

                    //search twohop neighbor entry
                    var twoHopBeighbor = TwoHopNeighborTable.GetEntryFor(twohopneighbor);

                    //upate two hop neighbors table
                    if (twoHopBeighbor == null)
                    {
                        // if neighbor not exists add it 
                        TwoHopNeighborTable.AddEntry(twohopneighbor, message.Sender);
                        IsHelloUpdate = true;
                    }
                    else
                    {
                        // if neighbor exists check if sender is listed in accessthrough
                        if (!twoHopBeighbor.AccessableThrough.Contains(message.Sender))
                        {
                            //if not add it to the accessable through
                            twoHopBeighbor.AccessableThrough.Add(message.Sender);
                            IsHelloUpdate = true;
                        }
                    }
                }
            }

            // if hello messages has info about multipoint relays
            if (olsrMessage.MultiPointRelays != null && olsrMessage.MultiPointRelays.Any())
            {
                // if this client was selected as mpr by sender of this message 
                if (olsrMessage.MultiPointRelays.Contains(Client.Id))
                {
                    if (!MultiPointRelaySelectorSet.Contains(olsrMessage.Sender))
                    {
                        // add sender to mpr selection set
                        MultiPointRelaySelectorSet.Add(olsrMessage.Sender);
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
                //drop message 
                return;
            }

            //process tc message
            foreach (var mprSelector in topMessage.MultiPointRelaySelectorSet)
            {
                if (!ToplogyTable.Entries.Any(e => e.OrigniatorId.Equals(topMessage.Sender) && e.MprSelectorId.Equals(mprSelector)))
                {
                    ToplogyTable.AddEntry(topMessage.Sender, mprSelector);
                    IsTopologyUpdate = true;
                }
            }

            // A node P forwards control messages only from MPRsel(P)
            if (MultiPointRelaySelectorSet.Contains(topMessage.Sender))
            {
                //forward message
                BroadcastTopologyControlMessages(topMessage);
            }
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
                SendMessage(message);
            }
            else
            {
                Client.ReceiveData(message);
            }
        }

        /// <summary>
        /// Determines whether [is one hop neighbor] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool IsOneHopNeighbor(string id)
        {
            return OneHopNeighborTable.GetEntryFor(id) != null;
        }

        /// <summary>
        /// Determines whether [is two hop neighbor] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool IsTwoHopNeighbor(string id)
        {
            return TwoHopNeighborTable.GetEntryFor(id) != null;
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
            var entry = OneHopNeighborTable.GetEntryFor(id);

            return entry != null && entry.IsMultiPointRelay;
        }
    }
}
