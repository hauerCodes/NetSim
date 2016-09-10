// -----------------------------------------------------------------------
// <copyright file="DsrRoutingProtocol.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsrRoutingProtocol.cs</summary>
// -----------------------------------------------------------------------
// ReSharper disable UnusedMember.Local
namespace NetSim.Lib.Routing.DSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Routing.Helpers;
    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The dsr routing protocol implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimRoutingProtocol" />
    public class DsrRoutingProtocol : NetSimRoutingProtocol
    {
        /// <summary>
        /// The message handler resolver instance.
        /// </summary>
        private readonly MessageHandlerResolver handlerResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsrRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public DsrRoutingProtocol(NetSimClient client)
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
        /// Gets the output message queue (should be used only for data messages).
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
            this.Table = new DsrTable();

            // intialize request cache
            this.RequestCache = new List<NetSimRequestCacheEntry>();

            // intialize request id for route request identification 
            this.CurrentRequestId = 1;

            // intialize outgoing messages
            this.OutputQueue = new Queue<NetSimQueuedMessage>();

            // local table reference casted to the right type
            var localTableRef = (DsrTable)this.Table;

            // self routing entry with metric 0
            localTableRef.AddInitialRouteEntry(this.Client.Id, this.Client.Id, 0);
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public override void PerformRoutingStep()
        {
            // handle all incomming messages
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
        /// <returns>
        /// The string representation of the protocol specific routing data.
        /// </returns>
        protected override string GetRoutingData()
        {
            return this.Table.ToString();
        }

        /// <summary>
        /// Adds the cached request.
        /// </summary>
        /// <param name="reqMessage">The request message.</param>
        private void AddCachedRequest(DsrRouteRequestMessage reqMessage)
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
        /// Handles the outgoing DSR message.
        /// Note: Messages handled in this methods where initially send from another node.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        private void ForwardDsrMessage(NetSimQueuedMessage queuedMessage)
        {
            // searches a handler method with the dsrmessagehandler attribute and the 
            // right message type and for incoming(false) or outgoing (true, default) messages.
            // e.g. IncomingDsrRouteRequestMessageHandler
            var method = this.handlerResolver.GetHandlerMethod(queuedMessage.Message.GetType());

            method?.Invoke(this, new object[] { queuedMessage });
        }

        /// <summary>
        /// Gets the DSR cached route.
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        /// <returns>The found dsr table entry or null.</returns>
        private DsrTableEntry GetDsrRoute(string receiver)
        {
            return (DsrTableEntry)this.Table.GetRouteFor(receiver);
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

                // call handler
                method?.Invoke(this, new object[] { message });

                // ignore all not dsr messages - all messages have to be packed in dsrFrame                
            }
        }

        /// <summary>
        /// Handles the not reachable route.
        /// </summary>
        /// <param name="nextHopId">The next hop identifier.</param>
        /// <param name="frameMessage">The frame message.</param>
        private void HandleNotReachableRoute(string nextHopId, DsrFrameMessage frameMessage)
        {
            var dsrTable = this.Table as DsrTable;

            // remove notreachable route 
            dsrTable?.HandleError(this.Client.Id, nextHopId);

            // broadcast route error to neighbors - Neighbors remove every route with sender -> notreachable in path
            this.Client.BroadcastMessage(
                new DsrRouteErrorMessage() { Sender = this.Client.Id, NotReachableNode = nextHopId, });

            if (frameMessage != null)
            {
                // send route error to sender with back path in message 
                // receiver removes every route with sender - notreachable in path
                this.SendMessage(
                    new DsrRouteErrorMessage()
                    {
                        Sender = this.Client.Id,
                        Receiver = frameMessage.Sender,
                        Route = frameMessage.Route,
                        NotReachableNode = nextHopId,
                        FailedMessage = frameMessage.Data
                    });
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

                // check if message is a dsr message
                if (this.IsDsrMessage(queuedMessage.Message))
                {
                    // handle "resend" of dsr message intialy send from other node
                    this.ForwardDsrMessage(queuedMessage);
                }
                else
                {
                    // if here messages gets initally send from this node
                    this.HandleRouteDiscoveryForOutgoingMessage(queuedMessage);
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
            // search the route for message (also in cache)
            var searchedRoute = this.GetDsrRoute(queuedMessage.Message.Receiver);

            // if route found - send the message via the connection
            if (searchedRoute != null)
            {
                var sendMessage = queuedMessage.Message;

                // pack message in dsrframemessage and set found route
                var dsrFrame = new DsrFrameMessage()
                {
                    Data = (NetSimMessage)sendMessage.Clone(),
                    Receiver = sendMessage.Receiver,
                    Sender = sendMessage.Sender,
                    Route = new List<string>(searchedRoute.Route),
                };

                // determine the next hop of the requested route
                var nextHopId = dsrFrame.GetNextHop(this.Client.Id);

                // lay message on wire - intial send
                if (this.IsConnectionReachable(nextHopId))
                {
                    this.Client.Connections[nextHopId].StartTransportMessage(dsrFrame, this.Client.Id, nextHopId);
                }
                else
                {
                    this.HandleNotReachableRoute(nextHopId, null);
                }
            }
            else
            {
                // if route not found and route discovery is not started for this message
                if (!queuedMessage.IsRouteDiscoveryStarted)
                {
                    // mark as started
                    queuedMessage.IsRouteDiscoveryStarted = true;

                    // broadcast to all neighbors
                    this.Client.BroadcastMessage(
                        new DsrRouteRequestMessage()
                        {
                            Sender = this.Client.Id,
                            Receiver = queuedMessage.Message.Receiver,
                            RequestId = this.CurrentRequestId,
                            Nodes = { this.Client.Id }
                        },
                        false);

                    // increase route request id
                    this.CurrentRequestId++;
                }

                // and enqueue message again
                this.OutputQueue.Enqueue(queuedMessage);
            }
        }

        /// <summary>
        /// Determines whether this protocol instance has cached request the specified request id.
        /// </summary>
        /// <param name="reqMessaged">The request messaged.</param>
        /// <returns>
        ///   <c>true</c> if has cached request with the specified request message; otherwise, <c>false</c>.
        /// </returns>
        private bool HasCachedRequest(DsrRouteRequestMessage reqMessaged)
        {
            return
                this.RequestCache
                .FirstOrDefault(r => r.Id.Equals(reqMessaged.Sender))?.HasCachedRequest(reqMessaged.RequestId) ?? false;
        }

        /// <summary>
        /// Handles the Incoming the DSR frame message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(DsrFrameMessage), Outgoing = false)]
        private void IncomingDsrFrameMessageHandler(NetSimMessage message)
        {
            // forward message if client is not reciever
            if (!message.Receiver.Equals(this.Client.Id))
            {
                this.SendMessage(message);
            }
            else
            {
                // unpack mesage from dsrframe
                var dsrFrame = (DsrFrameMessage)message;

                // forward message to client
                this.Client.ReceiveData(dsrFrame.Data);
            }
        }

        /// <summary>
        /// Handles the incoming the DSR route remove message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(DsrRouteErrorMessage), Outgoing = false)]
        private void IncomingDsrRouteErrorMessageHandler(NetSimMessage message)
        {
            DsrTable dsrTable = this.Table as DsrTable;
            DsrRouteErrorMessage errorMessage = (DsrRouteErrorMessage)message;

            // delete the (cached) routes defined by the error message from table 
            dsrTable?.HandleError(errorMessage.Sender, errorMessage.NotReachableNode);

            // check if the respone is for this node
            if (errorMessage.Receiver.Equals(this.Client.Id))
            {
                // check if error has failed message
                if (errorMessage.FailedMessage != null)
                {
                    // try to retransmit the failed message - start route discovery again
                    this.SendMessage(errorMessage.FailedMessage);
                }
            }
            else
            {
                // forward message
                this.SendMessage(errorMessage);
            }
        }

        /// <summary>
        /// Handles the  Incomings the DSR route response message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(DsrRouteReplyMessage), Outgoing = false)]
        private void IncomingDsrRouteReplyMessageHandler(NetSimMessage message)
        {
            DsrTable dsrTable = this.Table as DsrTable;
            DsrRouteReplyMessage repMessage = (DsrRouteReplyMessage)message;

            // handle route caching
            dsrTable?.HandleReplyRouteCaching(repMessage, this.Client.Id);

            // check if the respone is for this node
            if (repMessage.Receiver.Equals(this.Client.Id))
            {
                // save found route to table
                dsrTable?.HandleResponse(repMessage);
            }
            else
            {
                // forward message
                this.SendMessage(repMessage);
            }
        }

        /// <summary>
        /// Handles the incoming dsr route request message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MessageHandler(typeof(DsrRouteRequestMessage), Outgoing = false)]
        private void IncomingDsrRouteRequestMessageHandler(NetSimMessage message)
        {
            DsrTable dsrTable = (DsrTable)this.Table;
            DsrRouteRequestMessage reqMessage = (DsrRouteRequestMessage)message;

            // if duplicate
            if (this.HasCachedRequest(reqMessage))
            {
                // ignore message and proceed
                return;
            }

            // add request to cache
            this.AddCachedRequest(reqMessage);

            // add this node id to message Route
            reqMessage.Nodes.Add(this.Client.Id);

            // if this node was sender of request - ignore
            if (this.IsOwnRequest(reqMessage))
            {
                return;
            }

            // cache route
            dsrTable.HandleRequestRouteCaching(reqMessage);

            // check if message destination is current node (me)
            if (reqMessage.Receiver.Equals(this.Client.Id))
            {
                // send back rrep mesage the reverse way with found route 
                var response = new DsrRouteReplyMessage()
                {
                    Receiver = reqMessage.Sender,
                    Sender = this.Client.Id,
                    Route = new List<string>(reqMessage.Nodes)
                };

                // enqueue message for sending
                this.SendMessage(response);

                return;
            }
            else
            {
                // Check if route to the end destination for request is cached
                var route = this.Table.GetRouteFor(reqMessage.Receiver);

                if (route != null)
                {
                    var dsrRoute = (DsrTableEntry)route;

                    var newRoute = new List<string>(reqMessage.Nodes);

                    // remove last entry
                    newRoute.RemoveAt(newRoute.Count - 1);

                    // add cached route entries
                    newRoute.AddRange(dsrRoute.Route);

                    // send back rrep mesage the reverse way with found route 
                    // note: sender is the orig. receiver of the req
                    var response = new DsrRouteReplyMessage()
                    {
                        Receiver = reqMessage.Sender,
                        Sender = reqMessage.Receiver,
                        Route = newRoute
                    };

                    // enqueue message for sending
                    this.SendMessage(response);

                    return;
                }
            }

            // forward message to outgoing messages
            this.SendMessage(reqMessage);
        }

        /// <summary>
        /// Determines whether if message type is a DSR message type.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if is a dsr message with the specified message; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDsrMessage(NetSimMessage message)
        {
            List<Type> dsrTypes = new List<Type>()
                                  {
                                      typeof(DsrRouteReplyMessage),
                                      typeof(DsrRouteRequestMessage),
                                      typeof(DsrRouteErrorMessage),
                                      typeof(DsrFrameMessage)
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
        private bool IsOwnRequest(DsrRouteRequestMessage reqMessage)
        {
            return reqMessage.Nodes.Count > 0 && reqMessage.Nodes[0].Equals(this.Client.Id);
        }

        /// <summary>
        /// The outgoing DSR frame message handler.
        /// dsr frame - forward to destination
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(DsrFrameMessage), Outgoing = true)]
        private void OutgoingDsrFrameMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            // get dsr frame message instacne
            var frameMessage = (DsrFrameMessage)queuedMessage.Message;

            // get next hop from in frame message saved route info
            var nextHop = frameMessage.GetNextHop(this.Client.Id);

            if (this.IsConnectionReachable(nextHop))
            {
                // lay message "on" wire - start transmitting via connection
                this.Client.Connections[nextHop].StartTransportMessage(frameMessage, this.Client.Id, nextHop);
            }
            else
            {
                this.HandleNotReachableRoute(nextHop, frameMessage);
            }
        }

        /// <summary>
        /// Handles the outgoing DSR remove route messages.
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(DsrRouteErrorMessage), Outgoing = true)]
        private void OutgoingDsrRouteErrorMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            var errorMessage = (DsrRouteErrorMessage)queuedMessage.Message;

            // get the next hop id from the route info saved within this message
            string nextHopId = errorMessage.GetNextReverseHop(this.Client.Id);

            // if client has this connection (e.g. not deleted) and connection is not offline
            if (this.IsConnectionReachable(nextHopId))
            {
                // start message transport
                this.Client.Connections[nextHopId].StartTransportMessage(errorMessage, this.Client.Id, nextHopId);
            }
        }

        /// <summary>
        /// Handles the outgoing DsrRouteReplyMessage.
        /// dsr route reply - forward the reverse route way
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(DsrRouteReplyMessage), Outgoing = true)]
        private void OutgoingDsrRouteReplyMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            var responseMessage = (DsrRouteReplyMessage)queuedMessage.Message;

            // get the next hop id from the route info saved within this message
            string nextHopId = responseMessage.GetNextReverseHop(this.Client.Id);

            if (this.IsConnectionReachable(nextHopId))
            {
                // start message transport
                this.Client.Connections[nextHopId].StartTransportMessage(responseMessage, this.Client.Id, nextHopId);
            }
            else
            {
                this.HandleNotReachableRoute(nextHopId, null);
            }
        }

        /// <summary>
        /// Handles the outgoing dsr frame message.
        /// dsr route request - send to every neighbor
        /// </summary>
        /// <param name="queuedMessage">The queued message.</param>
        [MessageHandler(typeof(DsrRouteRequestMessage), Outgoing = true)]
        private void OutgoingDsrRouteRequestMessageHandler(NetSimQueuedMessage queuedMessage)
        {
            // get dsr requestMessage instacne - note: the request message was alreay handled in incomming messages
            var requestMessage = (DsrRouteRequestMessage)queuedMessage.Message;

            // broadcast to all neighbors
            this.Client.BroadcastMessage(requestMessage, false);
        }
    }
}