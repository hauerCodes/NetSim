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
        private int periodicUpdateCounter = 10;

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
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public OlsrState State { get; set; }

        /// <summary>
        /// The indicator for hello update has been received
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is hello update; otherwise, <c>false</c>.
        /// </value>
        private bool IsHelloUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is toplogy control update.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is toplogy control update; otherwise, <c>false</c>.
        /// </value>
        private bool IsToplogyUpdate { get; set; }

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
            
            //prepare the output message queue
            this.OutputQueue = new Queue<NetSimMessage>();

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
            IsToplogyUpdate = false;

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

                    if (IsHelloUpdate)
                    {
                        State = OlsrState.Hello;
                    }
                    else
                    {
                        this.State = OlsrState.Calculate;
                    }

                    break;

                case OlsrState.Calculate:
                    // search mpr and calculate routing table
                    var mprs = CalculateMultiPointRelays();

                    // set one hop neighbors to selected multi point relays
                    OneHopNeighborTable.Entries.ForEach((e) =>
                    {
                        e.IsMultiPointRelay = mprs.Contains(e.NeighborId);
                    });

                    this.State = OlsrState.TopologyControl;

                    break;

                case OlsrState.TopologyControl:
                    HandleIncomingMessages();

                    if (IsHelloUpdate)
                    {
                        State = OlsrState.Hello;
                    }

                    // restart hello process
                    if (stepCounter % periodicUpdateCounter == 0)
                    {
                        State = OlsrState.Hello;
                    }

                    break;
            }

            stepCounter++;
        }

        private void CalculateRoutingTable()
        {
            //throw new NotImplementedException();
        }

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
            string nextHopId = GetRoute(message.Receiver);

            Client.Connections[nextHopId].StartTransportMessage(message, this.Client.Id, nextHopId);
        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns></returns>
        protected override string GetRoutingData()
        {
            return $"{Table}\n\nOne-Hop Neighbor\nId Type\n{OneHopNeighborTable}\nTwo-Hop Neighbor\nId Trough\n{TwoHopNeighborTable}";
        }

        /// <summary>
        /// Broadcasts the hello messages.
        /// </summary>
        private void BroadcastHelloMessages()
        {
            // send hello message with all direct neighbors of this client
            Client.BroadcastMessage(new OlsrHelloMessage()
            {
                Neighbors = OneHopNeighborTable.Entries.Select(e => e.NeighborId).ToList()
            });
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
                    }
                    else
                    {
                        // if neighbor exists check if sender is listed in accessthrough
                        if (!twoHopBeighbor.AccessableThrough.Contains(message.Sender))
                        {
                            //if not add it to the accessable through
                            twoHopBeighbor.AccessableThrough.Add(message.Sender);
                        }
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
            throw new NotImplementedException();
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

        ///// <summary>
        ///// Handles the outgoing messages.
        ///// </summary>
        //private void HandleOutgoingMessages()
        //{
        //    // get the count of queued messages 
        //    int counter = OutputQueue.Count;

        //    // run for each queued message
        //    while (counter > 0)
        //    {
        //        // get next queued message
        //        var queuedMessage = OutputQueue.Dequeue();

        //        ////check if message is a dsr message
        //        //if (IsDsrMessage(queuedMessage.Message))
        //        //{
        //        //    // handle "resend" of dsr message intialy send from other node
        //        //    ForwardDsrMessage(queuedMessage);
        //        //}
        //        //else
        //        //{
        //        //    //if here messages gets initally send from this node
        //        //    HandleRouteDiscoveryForOutgoingMessage(queuedMessage);
        //        //}

        //        counter--;
        //    }
        //}

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
