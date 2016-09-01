using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Routing.Helpers;
using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;
using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// The message handler resolver instance.
        /// </summary>
        private readonly MessageHandlerResolver handlerResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AodvRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public AodvRoutingProtocol(NetSimClient client) : base(client)
        {
            handlerResolver = new MessageHandlerResolver(this.GetType());
        }

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
        public List<NetSimRequestCacheEntry> RequestCache { get; private set; }

        /// <summary>
        /// Gets or sets the current request identifier.
        /// </summary>
        /// <value>
        /// The current request identifier.
        /// </value>
        public int CurrentRequestId { get; set; }

        /// <summary>
        /// Gets or sets the current sequence.
        /// </summary>
        /// <value>
        /// The current sequence.
        /// </value>
        public AodvSequence CurrentSequence { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            //intialize routing table
            this.Table = new AodvTable();

            //intialize request cache
            this.RequestCache = new List<NetSimRequestCacheEntry>();

            //intialize outgoing messages
            this.OutputQueue = new Queue<NetSimQueuedMessage>();

            // intialize request id for route request identification 
            this.CurrentRequestId = 1;

            // create current (initial) sequence nr (ID-000)
            this.CurrentSequence = new AodvSequence(this.Client.Id, 0);

            // local table reference casted to the right type
            var localTableRef = (AodvTable)this.Table;

            // self routing entry with metric 0
            localTableRef.AddInitialRouteEntry(Client.Id, Client.Id, 0, CurrentSequence);
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
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
            int counter = OutputQueue.Count;

            while (counter > 0)
            {
                // get next queued message
                var queuedMessage = OutputQueue.Dequeue();

                ////search the route (next hop)
                //string nextHopId = GetRoute(queuedMessage.Message.Receiver);

                //// if route found - send the message via the connection
                //if (!string.IsNullOrEmpty(nextHopId))
                //{
                //    Client.Connections[nextHopId].StartTransportMessage(queuedMessage.Message, this.Client.Id, nextHopId);
                //}
                //else
                //{
                //    //if route not found start route discovery


                //    // and enqueue message again
                //    OutputQueue.Enqueue(queuedMessage);
                //}
                counter--;
            }
        }

        /// <summary>
        /// Handles the incomming messages.
        /// </summary>
        private void HandleIncommingMessages()
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

                //call handler
                method?.Invoke(this, new object[] { message });

                // if method not found - use default method to handle message (e.g. data mesage)
                DefaultIncomingMessageHandler(message);
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
        /// Handles the new outgoing message.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        private void HandleRouteDiscoveryForOutgoingMessage(NetSimQueuedMessage queuedMessage)
        {
            ////search the route for message (also in cache)
            //var searchedRoute = GetDsrRoute(queuedMessage.Message.Receiver);

            //// if route found - send the message via the connection
            //if (searchedRoute != null)
            //{
            //    var sendMessage = queuedMessage.Message;

            //    //pack message in dsrframemessage and set found route
            //    var dsrFrame = new DsrFrameMessage()
            //    {
            //        Data = (NetSimMessage)sendMessage.Clone(),
            //        Receiver = sendMessage.Receiver,
            //        Sender = sendMessage.Sender,
            //        Route = new List<string>(searchedRoute.Route),
            //    };

            //    //determine the next hop of the requested route
            //    var nextHopId = dsrFrame.GetNextHop(Client.Id);

            //    //lay message on wire - intial send
            //    Client.Connections[nextHopId].StartTransportMessage(dsrFrame, this.Client.Id, nextHopId);
            //}
            //else
            //{
            //    // if route not found and route discovery is not started for this message
            //    if (!queuedMessage.IsRouteDiscoveryStarted)
            //    {
            //        // mark as started
            //        queuedMessage.IsRouteDiscoveryStarted = true;

            //        //broadcast to all neighbors
            //        Client.BroadcastMessage(new DsrRouteRequestMessage()
            //        {
            //            Sender = Client.Id,
            //            Receiver = queuedMessage.Message.Receiver,
            //            RequestId = this.CurrentRequestId,
            //            Nodes = { this.Client.Id }
            //        }, false);

            //        //increase route request id
            //        this.CurrentRequestId++;
            //    }

            //    // and enqueue message again
            //    OutputQueue.Enqueue(queuedMessage);
            //}
        }

        /// <summary>
        /// Handles the outgoing dsr frame message.
        /// dsrrouterequest - send to every neighbor
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(AodvRouteRequestMessage), Outgoing = true)]
        private void OutgoingAodvRouteRequestMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            //get dsr requestMessage instacne - note: the request message was alreay handled in incomming messages
            var requestMessage = (AodvRouteRequestMessage)queuedMessage.Message;

            //broadcast to all neighbors
            Client.BroadcastMessage(requestMessage, false);
        }

        /// <summary>
        /// Handles the outgoing DsrRouteReplyMessage.
        /// dsrrouterespone - forward the reverse route way
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(AodvRouteReplyMessage), Outgoing = true)]
        private void OutgoingAodvRouteReplyMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            var responseMessage = (AodvRouteReplyMessage)queuedMessage.Message;

            // get the next hop id from the route table (reverse route)
            //string nextHopId = responseMessage.GetNextReverseHop(Client.Id);

            //if (!Client.Connections[nextHopId].IsOffline)
            //{
            //    // start message transport
            //    Client.Connections[nextHopId].StartTransportMessage(responseMessage, this.Client.Id, nextHopId);
            //}
            //else
            //{
            //    // broadcast route error to neighbors
            //}
        }

        /// <summary>
        /// Handles the incomming dsr route request message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvRouteRequestMessage), Outgoing = false)]
        private void IncomingAodvRouteRequestMessageHandler(NetSimMessage message)
        {
            AodvTable aodvTable = (AodvTable)Table; 
            AodvRouteRequestMessage reqMessage = (AodvRouteRequestMessage)message;

            //if duplicate
            if (HasCachedRequest(reqMessage))
            {
                //ignore message and proceed
                return;
            }

            //add request to cache
            AddCachedRequest(reqMessage);

            //add reverse routing entry - if route does'nt exist or sequencenr is newer
            aodvTable.AddRouteEntry(reqMessage.Sender,
                reqMessage.LastHop,
                reqMessage.HopCount + 1,
                (AodvSequence)reqMessage.SenderSequenceNr.Clone());

            // update request message - increase hopcount and update last hop
            reqMessage.LastHop = this.Client.Id;
            reqMessage.HopCount += 1; 

            //if this node was sender of request - ignore
            if (IsOwnRequest(reqMessage))
            {
                return;
            }

            //check if message destination is current node (me)
            if (reqMessage.Receiver.Equals(this.Client.Id))
            {
                //send back rrep mesage the reverse way with found route 
                var response = new AodvRouteReplyMessage()
                {
                    Receiver = reqMessage.Sender,
                    Sender = Client.Id,
                };

                //enqueue message for sending
                SendMessage(response);

                return;
            }

            // TODO Check if route to the end destination for request is cached

            // forward message to outgoing messages
            SendMessage(reqMessage);
        }

        /// <summary>
        /// Determines whether the given dsr rreq message is a own request.
        /// </summary>
        /// <param name="reqMessage">The req message.</param>
        /// <returns>
        ///   <c>true</c> if is own request; otherwise, <c>false</c>.
        /// </returns>
        private bool IsOwnRequest(AodvRouteRequestMessage reqMessage)
        {
            return reqMessage.Sender.Equals(this.Client.Id);
        }

        /// <summary>
        /// Handles the  Incomings the DSR route response message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvRouteReplyMessage), Outgoing = false)]
        private void IncomingAodvRouteReplyMessageHandler(NetSimMessage message)
        {
            AodvRouteReplyMessage repMessage = (AodvRouteReplyMessage)message;

            //check if the respone is for this node
            if (repMessage.Receiver.Equals(Client.Id))
            {
                var aodvTable = Table as AodvTable;

                //TODO save found route to table
                //aodvTable?.HandleResponse(repMessage);
            }
            else
            {
                //TODO save route entry to table 

                // forward message
                SendMessage(repMessage);
            }
        }

        /// <summary>
        /// Handles the  Incommings the DSR route remove message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvRouteErrorMessage), Outgoing = false)]
        private void IncomingAodvRouteErrorMessageHandler(NetSimMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the cached request.
        /// </summary>
        /// <param name="reqMessage">The req message.</param>
        private void AddCachedRequest(AodvRouteRequestMessage reqMessage)
        {
            var nodeCache = RequestCache.FirstOrDefault(r => r.Id.Equals(reqMessage.Sender));

            if (nodeCache == null)
            {
                nodeCache = new NetSimRequestCacheEntry() { Id = reqMessage.Sender };

                RequestCache.Add(nodeCache);
            }

            nodeCache.ChachedRequests.Add(reqMessage.RequestId);
        }

        /// <summary>
        /// Determines whether this protocol instance has chached request the specified reqid.
        /// </summary>
        /// <param name="reqMessaged">The req messaged.</param>
        /// <returns></returns>
        private bool HasCachedRequest(AodvRouteRequestMessage reqMessaged)
        {
            return
                RequestCache
                .FirstOrDefault(r => r.Id.Equals(reqMessaged.Sender))?
                    .HasCachedRequest(reqMessaged.RequestId) ?? false;
        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns></returns>
        protected override string GetRoutingData()
        {
            //TODO
            return Table.ToString();
        }
    }
}
