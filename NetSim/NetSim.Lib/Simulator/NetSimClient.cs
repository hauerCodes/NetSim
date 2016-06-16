using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using NetSim.Lib.Routing;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimClient : NetSimItem
    {
        /// <summary>
        /// Occurs when clientStateUpdate.
        /// </summary>
        private event Action ClientStateUpdate;

        /// <summary>
        /// The step counter
        /// </summary>
        private int stepCounter;

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimClient"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="location">The location.</param>
        public NetSimClient(string id, NetSimLocation location)
        {
            this.Id = id;
            this.Location = location;
            this.stepCounter = 0;
            this.InputQueue = new Queue<NetSimMessage>();
            this.Connections = new Dictionary<string, NetSimConnection>();
        }

        #endregion

        #region Properties

        public Dictionary<string, NetSimConnection> Connections { get; set; }

        public Queue<NetSimMessage> InputQueue { get; set; }

        public NetSimRoutingProtocol RoutingProtocol { get; set; }

        public int StepCounter => stepCounter;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when client state is updated due routing or other events.
        /// </summary>
        public event Action StateUpdated
        {
            add
            {
                ClientStateUpdate += value;
            }
            remove
            {
                ClientStateUpdate -= value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes the protocol.
        /// </summary>
        /// <param name="protocolType">Type of the protocol.</param>
        public void InitializeProtocol(NetSimProtocolType protocolType)
        {
            // (re)set step counter
            this.stepCounter = 0;

            // create protocoll instance
            this.RoutingProtocol = RoutingProtocolFactory.CreateInstance(protocolType, this);

            // (re)set input queue
            this.InputQueue = new Queue<NetSimMessage>();

            //initialize protocol
            RoutingProtocol.Initialize();
        }

        /// <summary>
        /// Broadcasts the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void BroadcastMessage(NetSimMessage message)
        {
            NetSimMessage localCopy = (NetSimMessage)message.Clone();

            foreach(var connection in Connections)
            {
                // insert receiver id 
                localCopy.Receiver = connection.Key;
                localCopy.Sender = Id;
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(NetSimMessage message)
        {
            string nextHopId = RoutingProtocol.GetRoute(message.Receiver);

            Connections[nextHopId].StartTransportMessage(message);
        }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ReceiveMessage(NetSimMessage message)
        {
            InputQueue?.Enqueue(message);
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public void PerformRoutingStep()
        {
            RoutingProtocol.PerformRoutingStep();

            stepCounter++;
        }

        /// <summary>
        /// Called when [state updated].
        /// </summary>
        protected void OnStateUpdated()
        {
            ClientStateUpdate?.Invoke();
        }
    }
}