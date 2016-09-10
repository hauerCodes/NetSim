// -----------------------------------------------------------------------
// <copyright file="AodvRoutingProtocol.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvRoutingProtocol.cs</summary>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local
namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Routing.Helpers;
    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The routing protocol class.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimRoutingProtocol" />
    public class AodvRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// The message handler resolver instance.
        /// </summary>
        private readonly MessageHandlerResolver handlerResolver;

        /// <summary>
        /// The periodic hello update counter
        /// </summary>
        private int periodicHelloUpdateCounter = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="AodvRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public AodvRoutingProtocol(NetSimClient client)
            : base(client)
        {
            this.handlerResolver = new MessageHandlerResolver(this.GetType());
        }

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
        /// Gets or sets the neighbor links or connections.
        /// Key is the identifier of the node.
        /// Value is the time since the last message was received from this neighbor.
        /// </summary>
        /// <value>
        /// The neighbors.
        /// </value>
        public Dictionary<string, int> Neighbours { get; set; }

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
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            // intialize routing table
            this.Table = new AodvTable();

            // intialize request cache
            this.RequestCache = new List<NetSimRequestCacheEntry>();

            // intialize outgoing messages
            this.OutputQueue = new Queue<NetSimQueuedMessage>();

            // intialize the neighbour list
            this.Neighbours = new Dictionary<string, int>();

            // intialize request id for route request identification 
            this.CurrentRequestId = 1;

            // create current (initial) sequence nr (ID-000)
            this.CurrentSequence = new AodvSequence(this.Client.Id, 0);

            // local table reference casted to the right type
            var localTableRef = (AodvTable)this.Table;

            // self routing entry with metric 0
            localTableRef.AddRouteEntry(this.Client.Id, this.Client.Id, 0, this.CurrentSequence);
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // Handle Route maintaince
            this.HandleRouteMaintenance();

            // handle all incoming messages
            this.HandleIncomingMessages();

            // handle outgoing queued messages
            this.HandleOutgoingMessages();

            this.StepCounter++;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void SendMessage(NetSimMessage message)
        {
            // queue message
            this.OutputQueue.Enqueue(new NetSimQueuedMessage() { Message = message, });
        }

        /// <summary>
        /// Gets the routing data.
        /// </summary>
        /// <returns>The string representation of the routing data.</returns>
        protected override string GetRoutingData()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(this.Table.ToString());

            if (this.Neighbours.Count > 0)
            {
                builder.AppendLine("Neighbours");
                builder.AppendLine("Node TTL");

                this.Neighbours.Keys.ToList()
                    .ForEach(key => builder.AppendFormat("{0,4} {1,3}\n", key, this.Neighbours[key]));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Adds the cached request.
        /// </summary>
        /// <param name="reqMessage">The request message.</param>
        private void AddCachedRequest(AodvRouteRequestMessage reqMessage)
        {
            var nodeCache = this.RequestCache.FirstOrDefault(r => r.Id.Equals(reqMessage.Sender));

            if (nodeCache == null)
            {
                nodeCache = new NetSimRequestCacheEntry() { Id = reqMessage.Sender };

                this.RequestCache.Add(nodeCache);
            }

            nodeCache.CachedRequests.Add(reqMessage.RequestId);
        }

        /// <summary>
        /// Handles the received message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DefaultIncomingMessageHandler(NetSimMessage message)
        {
            // handle the neighbour lsit update - inactive timer management 
            this.UpdateNeighbourList(message.Sender);

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
        /// Handles the outgoing DSR message.
        /// Note: Messages handled in this methods where initially send from another node.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        private void ForwardAodvMessage(NetSimQueuedMessage queuedMessage)
        {
            // searches a handler method with the messagehandler attribute and the 
            // right message type and for incoming(false) or outgoing (true, default) messages.
            // e.g. IncomingAodvRouteRequestMessageHandler
            var method = this.handlerResolver.GetHandlerMethod(queuedMessage.Message.GetType());

            method?.Invoke(this, new object[] { queuedMessage });
        }

        /// <summary>
        /// Handles the incomings messages.
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
        /// Handles the outgoing messages.
        /// </summary>
        private void HandleOutgoingMessages()
        {
            // get the count of queued messages 
            int counter = this.OutputQueue.Count;

            // run for each queued message
            while (counter > 0)
            {
                // get next queued message
                var queuedMessage = this.OutputQueue.Dequeue();

                if (this.IsAodvMessage(queuedMessage.Message))
                {
                    // handle "resend" of dsr message intialy send from other node
                    this.ForwardAodvMessage(queuedMessage);
                }
                else
                {
                    var aodvTable = (AodvTable)this.Table;

                    // search the route (next hop)
                    string nextHopId = this.GetRoute(queuedMessage.Message.Receiver);

                    // add the sender of this message as active neighbour for route
                    aodvTable.AddActiveNeigbour(queuedMessage.Message.Receiver, queuedMessage.Message.Sender);

                    // if route not found
                    if (string.IsNullOrEmpty(nextHopId))
                    {
                        // try to search the route
                        this.HandleRouteDiscoveryForOutgoingMessage(queuedMessage);
                    }
                    else
                    {
                        // if connection is not offline and not deleted
                        if (this.IsConnectionReachable(nextHopId))
                        {
                            // if route was found  - send message along this route
                            this.Client.Connections[nextHopId].StartTransportMessage(
                                queuedMessage.Message,
                                this.Client.Id,
                                nextHopId);
                        }
                        else
                        {
                            this.HandleRouteError(
                                nextHopId,
                                queuedMessage.Message.Sender,
                                queuedMessage.Message.Receiver);
                        }
                    }
                }

                counter--;
            }
        }

        /// <summary>
        /// Handles the new outgoing message.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        private void HandleRouteDiscoveryForOutgoingMessage(NetSimQueuedMessage queuedMessage)
        {
            // if route not found and route discovery is not started for this message
            if (!queuedMessage.IsRouteDiscoveryStarted)
            {
                // mark as started
                queuedMessage.IsRouteDiscoveryStarted = true;

                // increment SequenceNr every time a rreq gets sent
                this.CurrentSequence.SequenceNr += 2;

                // broadcast to all neighbors
                this.Client.BroadcastMessage(
                    new AodvRouteRequestMessage()
                    {
                        Sender = this.Client.Id,
                        Receiver = queuedMessage.Message.Receiver,
                        RequestId = this.CurrentRequestId,
                        SenderSequenceNr = (AodvSequence)this.CurrentSequence.Clone(),
                        LastHop = this.Client.Id
                    },
                    false);

                // increase route request id
                this.CurrentRequestId++;
            }

            // and enqueue message again
            this.OutputQueue.Enqueue(queuedMessage);
        }

        /// <summary>
        /// Handles the route error.
        /// </summary>
        /// <param name="nextHopId">The next hop identifier.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="destination">The destination.</param>
        private void HandleRouteError(string nextHopId, string sender, string destination)
        {
            // is is a direct neighbor set leave time zero
            if (this.Neighbours.ContainsKey(nextHopId))
            {
                this.Neighbours[nextHopId] = 0;
            }

            // send route error message
            this.SendMessage(
                new AodvRouteErrorMessage()
                {
                    Sender = this.Client.Id,
                    Receiver = sender,
                    UnReachableDestination = destination,
                    UnReachableDestinationSequenceNr = (AodvSequence)this.CurrentSequence.Clone()
                });
        }

        /// <summary>
        /// Handles the route maintenance.
        /// </summary>
        private void HandleRouteMaintenance()
        {
            // if neighbour list is emtpy - return
            if (this.Neighbours.Count == 0)
            {
                return;
            }

            AodvTable aodvTable = (AodvTable)this.Table;

            // after a period of time perform route maintaince via hello messages
            if (this.StepCounter % this.periodicHelloUpdateCounter == 0)
            {
                // broadcast send hello messages to every neighbours
                foreach (var neighborId in this.Neighbours.Keys)
                {
                    // enqueue the message for sending 
                    this.SendMessage(
                        new AodvHelloMessage()
                        {
                            Sender = this.Client.Id,
                            Receiver = neighborId,
                            SenderSequenceNr = (AodvSequence)this.CurrentSequence.Clone()
                        });
                }
            }

            // decrease the neigbor response time (clone key list)
            foreach (string key in this.Neighbours.Keys.ToList())
            {
                this.Neighbours[key] -= 1;
            }

            // select neighbours where the inactive coutner is zero or less
            var inactiveNeighbours = this.Neighbours.Where(n => n.Value <= 0).Select(n => n.Key).ToList();

            // if a neighbour was for a period of time inactive (hellotimer * 2)
            if (inactiveNeighbours.Any())
            {
                foreach (var neighbourId in inactiveNeighbours)
                {
                    var routeErrorReceivers = aodvTable.HandleRouteMaintaince(neighbourId);

                    // enqueue route error messages for sending to route error receivers
                    routeErrorReceivers.ForEach(
                        receiver =>
                            this.SendMessage(
                                new AodvRouteErrorMessage()
                                {
                                    Sender = this.Client.Id,
                                    Receiver = receiver,
                                    UnReachableDestination = neighbourId,
                                    UnReachableDestinationSequenceNr = (AodvSequence)this.CurrentSequence.Clone()
                                }));

                    // remove the inactive neighbour
                    this.Neighbours.Remove(neighbourId);
                }
            }
        }

        /// <summary>
        /// Determines whether this protocol instance has a cached request the specified request id.
        /// </summary>
        /// <param name="reqMessaged">The request messaged.</param>
        /// <returns>
        ///   <c>true</c> if is cached request; otherwise, <c>false</c>.
        /// </returns>
        private bool HasCachedRequest(AodvRouteRequestMessage reqMessaged)
        {
            return this.RequestCache
                .FirstOrDefault(r => r.Id.Equals(reqMessaged.Sender))?.HasCachedRequest(reqMessaged.RequestId) ?? false;
        }

        /// <summary>
        /// Handles the  Incoming the aodv hello message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvHelloMessage), Outgoing = false)]
        private void IncomingAodvHelloMessageHandler(NetSimMessage message)
        {
            AodvHelloMessage helloMessage = (AodvHelloMessage)message;
            AodvTable aodvTable = (AodvTable)this.Table;

            // handle the neighbour lsit update - inactive timer management 
            this.UpdateNeighbourList(helloMessage.Sender);

            // search for a route for hello message sender
            AodvTableEntry route = (AodvTableEntry)aodvTable.GetRouteFor(helloMessage.Sender);

            // create route for neighbour if not exists
            if (route == null)
            {
                aodvTable.AddRouteEntry(
                    helloMessage.Sender,
                    helloMessage.Sender,
                    1,
                    (AodvSequence)helloMessage.SenderSequenceNr.Clone());
            }
        }

        /// <summary>
        /// Handles the  Incomings the DSR route remove message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvRouteErrorMessage), Outgoing = false)]
        private void IncomingAodvRouteErrorMessageHandler(NetSimMessage message)
        {
            AodvRouteErrorMessage errorMessage = (AodvRouteErrorMessage)message;

            var aodvTable = this.Table as AodvTable;

            // drop if own forwarded message
            if (errorMessage.Sender.Equals(this.Client.Id))
            {
                return;
            }

            // drop every route with destination from errormessage
            List<string> receivers = aodvTable?.HandleRouteMaintaince(errorMessage.UnReachableDestination);

            // TODO check if the respone is not for this node . then forward
            // if (!errorMessage.Receiver.Equals(Client.Id))
            // {
            // // forward message
            // SendMessage(errorMessage);
            // }
            if (receivers == null)
            {
                return;
            }

            foreach (var receiver in receivers)
            {
                AodvRouteErrorMessage clone = (AodvRouteErrorMessage)errorMessage.Clone();
                clone.Receiver = receiver;

                this.SendMessage(clone);
            }
        }

        /// <summary>
        /// Handles the  Incomings the DSR route response message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvRouteReplyMessage), Outgoing = false)]
        private void IncomingAodvRouteReplyMessageHandler(NetSimMessage message)
        {
            AodvRouteReplyMessage repMessage = (AodvRouteReplyMessage)message;
            var aodvTable = this.Table as AodvTable;

            // handle the neighbour lsit update - inactive timer management 
            this.UpdateNeighbourList(repMessage.Sender);
            this.UpdateNeighbourList(repMessage.LastHop);

            // save found route to table
            aodvTable?.HandleReplyRoute(repMessage);

            // check if the respone is not for this node . then forward
            if (!repMessage.Receiver.Equals(this.Client.Id))
            {
                // update reply message - increase hopcount and update last hop
                repMessage.LastHop = this.Client.Id;
                repMessage.HopCount += 1;

                // forward message
                this.SendMessage(repMessage);
            }
        }

        /// <summary>
        /// Handles the incomings dsr route request message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(AodvRouteRequestMessage), Outgoing = false)]
        private void IncomingAodvRouteRequestMessageHandler(NetSimMessage message)
        {
            AodvTable aodvTable = (AodvTable)this.Table;
            AodvRouteRequestMessage reqMessage = (AodvRouteRequestMessage)message;

            // if this node was sender of request - or has already a cached version of request
            if (this.IsOwnRequest(reqMessage) || this.HasCachedRequest(reqMessage))
            {
                // ignore message and proceed
                return;
            }

            // add request to cache
            this.AddCachedRequest(reqMessage);

            // add reverse routing entry - if route doesn't exist or sequencenr is newer
            aodvTable.HandleRequestReverseRouteCaching(reqMessage);

            // update request message - increase hopcount and update last hop
            reqMessage.LastHop = this.Client.Id;
            reqMessage.HopCount += 1;

            // check if message destination is current node 
            if (reqMessage.Receiver.Equals(this.Client.Id))
            {
                // send back rrep mesage the reverse way 
                var response = new AodvRouteReplyMessage()
                {
                    Receiver = reqMessage.Sender,
                    Sender = this.Client.Id,
                    ReceiverSequenceNr = (AodvSequence)this.CurrentSequence.Clone(),
                    LastHop = this.Client.Id
                };

                // enqueue message for sending
                this.SendMessage(response);
            }
            else
            {
                // Check if route was cached
                var searchRoute = aodvTable.SearchCachedRoute(reqMessage);

                if (searchRoute != null)
                {
                    // send reply back to requester - send back rrep mesage the reverse way 
                    var response = new AodvRouteReplyMessage()
                    {
                        Receiver = reqMessage.Sender,
                        Sender = searchRoute.Destination,
                        ReceiverSequenceNr = (AodvSequence)searchRoute.SequenceNr.Clone(),
                        HopCount = searchRoute.Metric,
                        LastHop = this.Client.Id
                    };

                    // enqueue message for sending
                    this.SendMessage(response);
                }
                else
                {
                    // forward message to outgoing messages
                    this.SendMessage(reqMessage);
                }
            }
        }

        /// <summary>
        /// Determines whether if message type is a AODV message type.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if [is aodv message] [the specified message]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAodvMessage(NetSimMessage message)
        {
            List<Type> dsrTypes = new List<Type>()
                                  {
                                      typeof(AodvRouteReplyMessage),
                                      typeof(AodvHelloMessage),
                                      typeof(AodvRouteRequestMessage),
                                      typeof(AodvRouteErrorMessage)
                                  };

            Type messageType = message.GetType();
            return dsrTypes.Any(t => t == messageType);
        }

        /// <summary>
        /// Determines whether the given dsr route request message is a own request.
        /// </summary>
        /// <param name="reqMessage">The request message.</param>
        /// <returns>
        ///   <c>true</c> if is own request; otherwise, <c>false</c>.
        /// </returns>
        private bool IsOwnRequest(AodvRouteRequestMessage reqMessage)
        {
            return reqMessage.Sender.Equals(this.Client.Id);
        }

        /// <summary>
        /// Handles the outgoing aodv hello message.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(AodvHelloMessage), Outgoing = true)]
        private void OutgoingAodvHelloMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            // get AodvHelloMessage instacne 
            var helloMessage = (AodvHelloMessage)queuedMessage.Message;

            if (this.IsConnectionReachable(helloMessage.Receiver))
            {
                // start message transport
                this.Client.Connections[helloMessage.Receiver].StartTransportMessage(
                    helloMessage,
                    this.Client.Id,
                    helloMessage.Receiver);
            }
        }

        /// <summary>
        /// Handles the outgoing aodv route error message.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(AodvRouteErrorMessage), Outgoing = true)]
        private void OutgoingAodvRouteErrorMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            // get AodvHelloMessage instacne 
            var errorMessage = (AodvRouteErrorMessage)queuedMessage.Message;

            // indicates that the error messages was broadcasted
            if (this.Client.Connections.ContainsKey(errorMessage.Receiver))
            {
                if (this.IsConnectionReachable(errorMessage.Receiver))
                {
                    // start message transport
                    this.Client.Connections[errorMessage.Receiver].StartTransportMessage(
                        errorMessage,
                        this.Client.Id,
                        errorMessage.Receiver);
                }
            }
            else
            {
                // error message sgets forwared among active path
                var nextHopId = this.Table.GetRouteFor(errorMessage.Receiver)?.NextHop;

                if (nextHopId != null && this.IsConnectionReachable(nextHopId))
                {
                    // start message transport
                    this.Client.Connections[nextHopId].StartTransportMessage(errorMessage, this.Client.Id, nextHopId);
                }
            }
        }

        /// <summary>
        /// Handles the outgoing DsrRouteReplyMessage.
        /// dsr route reply - forward the reverse route way
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(AodvRouteReplyMessage), Outgoing = true)]
        private void OutgoingAodvRouteReplyMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            var aodvTable = (AodvTable)this.Table;
            var responseMessage = (AodvRouteReplyMessage)queuedMessage.Message;

            // get the next hop id from the route table (reverse route)
            string nextHopId = aodvTable.GetRouteFor(responseMessage.Receiver).NextHop;

            if (this.IsConnectionReachable(nextHopId))
            {
                // start message transport
                this.Client.Connections[nextHopId].StartTransportMessage(responseMessage, this.Client.Id, nextHopId);
            }
            else
            {
                this.HandleRouteError(nextHopId, responseMessage.Sender, responseMessage.Receiver);
            }
        }

        /// <summary>
        /// Handles the outgoing dsr frame message.
        /// dsr route request - send to every neighbor
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(AodvRouteRequestMessage), Outgoing = true)]
        private void OutgoingAodvRouteRequestMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            // get dsr requestMessage instacne - note: the request message was alreay handled in incomings messages
            var requestMessage = (AodvRouteRequestMessage)queuedMessage.Message;

            // broadcast to all neighbors
            this.Client.BroadcastMessage(requestMessage, false);
        }

        /// <summary>
        /// Updates the neighbour list.
        /// </summary>
        /// <param name="messageSenderId">The message sender identifier.</param>
        private void UpdateNeighbourList(string messageSenderId)
        {
            // if message is from this node - ignore
            if (this.Client.Id.Equals(messageSenderId))
            {
                return;
            }

            // if potential neighour is not direct connected - ignore the message
            if (!this.Client.Connections.ContainsKey(messageSenderId))
            {
                return;
            }

            // if neighbors contains message sender - update active neighbors lsit
            if (this.Neighbours.ContainsKey(messageSenderId))
            {
                // reset the inactive timer for the sender
                this.Neighbours[messageSenderId] = this.periodicHelloUpdateCounter * 2;
            }
            else
            {
                // add the sender to the neighbors list
                this.Neighbours.Add(messageSenderId, this.periodicHelloUpdateCounter * 2);
            }
        }
    }
}