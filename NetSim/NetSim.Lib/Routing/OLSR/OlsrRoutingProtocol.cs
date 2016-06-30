using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// Gets a value indicating whether this instance is initial broadcast.
        /// </summary>
        private bool isFirstBroadcastReady;

        /// <summary>
        /// The periodic update counter
        /// </summary>
        private int periodicUpdateCounter = 10;

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public OlsrRoutingProtocol(NetSimClient client) : base(client) { }

        #endregion

        #region Properties

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

        #endregion

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

            // set intial broadcast to false
            this.isFirstBroadcastReady = false;

            //set protocol state 
            this.State = OlsrState.Hello;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            switch (State)
            {
                case OlsrState.Hello:
                    // send hello message to all direct links
                    BroadcastHelloMessages();

                    State = OlsrState.ReceiveHello;
                    break;

                case OlsrState.ReceiveHello:
                    // wait for incoming hello messages 
                    if (HandleIncommingMessagesAndCheckForUpdate())
                    {
                        State = OlsrState.Hello;
                    }
                    else
                    {
                        this.State = OlsrState.Calculate;
                    }

                    //// wait unitl every connected links has sent his hello
                    //if (this.OneHopNeighborTable.Entries.Count
                    //    >= this.Client.Connections.Count(x => !x.Value.IsOffline))
                    //{
                    //    this.State = OlsrState.Hello;
                    //}

                    break;

                case OlsrState.Calculate:
                    // search mpr and calculate routing table


                    this.State = OlsrState.TopologyControl;
                    break;
                case OlsrState.TopologyControl:
                    if (HandleIncommingMessagesAndCheckForUpdate())
                    {
                        State = OlsrState.Hello;
                    }

                    // restart hello process
                    if (stepCounter % periodicUpdateCounter == 0)
                    {
                        State = OlsrState.Hello;
                        isFirstBroadcastReady = false;
                    }

                    break;
            }

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
        /// Handles the received hello message.
        /// </summary>
        /// <param name="message">The message.</param>
        private bool HandleReceivedHelloMessage(OlsrHelloMessage message)
        {
            bool helloUpdate = false;

            //upate one hop neighbors
            if (OneHopNeighborTable.GetEntryFor(message.Sender) == null)
            {
                OneHopNeighborTable.AddEntry(message.Sender);
                helloUpdate = true;
            }

            if (message.Neighbors != null && message.Neighbors.Any())
            {
                //update two hop neighbors
                foreach (string twohopneighbor in message.Neighbors)
                {
                    //if twohop neighbor is also one hop neighbor ignore entry 
                    if(OneHopNeighborTable.GetEntryFor(twohopneighbor) != null)
                    {
                        continue;
                    }

                    // if two hop neighbor is this client id itself - ingore entry
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

            return helloUpdate;
        }

        /// <summary>
        /// Handles the incomming messages.
        /// </summary>
        private bool HandleIncommingMessagesAndCheckForUpdate()
        {
            bool helloUpdate = false;

            if (Client.InputQueue.Count > 0)
            {
                while (Client.InputQueue.Count > 0)
                {
                    var message = Client.InputQueue.Dequeue();

                    // if message is update message
                    if (message is OlsrHelloMessage)
                    {
                        if (HandleReceivedHelloMessage((OlsrHelloMessage)message))
                        {
                            helloUpdate = true;
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

            return helloUpdate;
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
    }
}
