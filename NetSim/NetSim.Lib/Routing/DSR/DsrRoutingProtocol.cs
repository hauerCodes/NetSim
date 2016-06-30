using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrRoutingProtocol : NetSimRoutingProtocol
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DsrRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public DsrRoutingProtocol(NetSimClient client) : base(client) { }

        /// <summary>
        /// Gets the output queue.
        /// </summary>
        /// <value>
        /// The output queue.
        /// </value>
        public Queue<NetSimQueuedMessage> OutputQueue { get; private set; }

        /// <summary>
        /// Gets the request cache.
        /// </summary>
        /// <value>
        /// The request cache.
        /// </value>
        public List<DsrCacheEntry> RequestCache { get; private set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            //intialize routing table
            this.Table = new DsrTable();

            //intialize request cache
            this.RequestCache = new List<DsrCacheEntry>();

            //intialize outgoing messages
            this.OutputQueue = new Queue<NetSimQueuedMessage>();

            // local table reference casted to the right type
            var localTableRef = (DsrTable)this.Table;

            // self routing entry with metric 0
            localTableRef.AddInitialRouteEntry(Client.Id, Client.Id, 0);
        }

        public override void PerformRoutingStep()
        {
            //handle all incomming messages
            HandleIncommingMessages();

            //handle outgoing queued messages
            HandleOutgoingMessages();

            stepCounter++;
        }

        /// <summary>
        /// Handles the outgoing messages.
        /// </summary>
        private void HandleOutgoingMessages()
        {
            // get the count of queued messages 
            int counter = OutputQueue.Count;

            // run for each queued message
            while (counter > 0)
            {
                // get next queued message
                var queuedMessage = OutputQueue.Dequeue();

                //search the route (next hop)
                var searchedRoute = GetDsrRoute(queuedMessage.Message.Receiver);

                // if route found - send the message via the connection
                if (searchedRoute != null)
                {
                    //TODO "hack" to determine the receiver endpoint of message
                    //queuedMessage.Message.NextReceiver = nextHopId;

                    //Client.Connections[nextHopId].StartTransportMessage(queuedMessage.Message);
                }
                else
                {
                    // if route not found and route discovery is not started for this message
                    if(!queuedMessage.IsRouteDiscoveryStarted)
                    {
                        // mark as started
                        queuedMessage.IsRouteDiscoveryStarted = true;

                        //broadcast to all neighbors
                        Client.BroadcastMessage(new DsrRreqMessage() { });
                    }

                    // and enqueue message again
                    OutputQueue.Enqueue(queuedMessage);
                }
                counter--;
            }
        }

        private NetSimTableEntry GetDsrRoute(string receiver)
        {
            return Table.GetRouteFor(receiver);
        }

        private void HandleIncommingMessages()
        {
            if (Client.InputQueue.Count > 0)
            {
                while (Client.InputQueue.Count > 0)
                {
                    var message = Client.InputQueue.Dequeue();

                    // if message is DsrRreqMessage message
                    if (message is DsrRreqMessage)
                    {
                        DsrRreqMessage reqMessage = message as DsrRreqMessage;


                        //check if message destination is current node 
                        if (reqMessage.Receiver.Equals(this.Client.Id))
                        {
                            //send back rrep mesage the reverse way with found route 
                        }
                    }
                    else if (message is DsrRrepMessage)
                    {
                        DsrRrepMessage repMessage = message as DsrRrepMessage;

                        // forward message

                    }

                    else if (message is DsrRrerMessage)
                    {
                        //// client table
                        //var dsdvTable = Table as DsdvTable;

                        //// ReSharper disable once InvertIf
                        //if (dsdvTable != null)
                        //{
                        //    if (dsdvTable.HandleUpdate(message.Sender, (message as DsdvUpdateMessage).UpdateTable))
                        //    {
                        //        topologyChangeUpdate = true;
                        //    }
                        //}
                    }
                    else if (message is DsrDataMessage)
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
                    else
                    { 
                        
                    }
                }
            }
        }

        public override void SendMessage(NetSimMessage message)
        {
            // queue message
            OutputQueue.Enqueue(new NetSimQueuedMessage()
            {
                Message = message,
                //Route = Table.GetRouteFor(message.Sender),
            });
        }

        protected override string GetRoutingData()
        {
            //TODO
            return Table.ToString();
        }
    }
}
