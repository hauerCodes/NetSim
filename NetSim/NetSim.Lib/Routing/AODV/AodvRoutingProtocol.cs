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

        /// <summary>
        /// Gets the output queue.
        /// </summary>
        /// <value>
        /// The output queue.
        /// </value>
        public Queue<NetSimQueuedMessage> OutputQueue { get; private set; }

        #endregion

        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            //intialize routing table
            this.Table = new AodvTable();

            //intialize outgoing messages
            this.OutputQueue = new Queue<NetSimQueuedMessage>();

            var localTableRef = (AodvTable)this.Table;

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

        private void HandleOutgoingMessages()
        {
            int counter = OutputQueue.Count;
            
            while(counter > 0)
            {
                // get next queued message
                var queuedMessage = OutputQueue.Dequeue();

                //search the route (next hop)
                string nextHopId = GetRoute(queuedMessage.Message.Receiver);

                // if route found - send the message via the connection
                if (!string.IsNullOrEmpty(nextHopId))
                {
                    //"hack" to determine the receiver endpoint of message
                    queuedMessage.Message.NextReceiver = nextHopId;

                    Client.Connections[nextHopId].StartTransportMessage(queuedMessage.Message);
                }
                else
                {
                    //if route not found start route discovery


                    // and enqueue message again
                    OutputQueue.Enqueue(queuedMessage);
                }
                counter--;
            }
        }

        private void HandleIncommingMessages()
        {
            if (Client.InputQueue.Count > 0)
            {
                while (Client.InputQueue.Count > 0)
                {
                    var message = Client.InputQueue.Dequeue();

                    // if message is AodvRreqMessage message
                    if (message is AodvRreqMessage)
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
                    else if (message is AodvRrepMessage)
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
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendMessage(NetSimMessage message)
        {
            // queue message
            OutputQueue.Enqueue(new NetSimQueuedMessage()
            {
                Message = message,
                //Route = Table.GetRouteFor(message.Sender),
            });

        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns></returns>
        protected override string GetRoutingData()
        {
            return Table.ToString(); 
        }
    }
}
