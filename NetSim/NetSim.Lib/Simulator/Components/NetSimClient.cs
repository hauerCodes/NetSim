// -----------------------------------------------------------------------
// <copyright file="NetSimClient.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimClient.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Components
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    using NetSim.Lib.Annotations;
    using NetSim.Lib.Routing.Helpers;
    using NetSim.Lib.Visualization;

    /// <summary>
    /// The client class that represents a node in the network.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimItem" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="NetSim.Lib.Simulator.INetSimConnectionEndpoint" />
    public class NetSimClient : NetSimItem, INotifyPropertyChanged, INetSimConnectionEndpoint
    {
        /// <summary>
        /// The client data
        /// </summary>
        private StringBuilder clientData;

        /// <summary>
        /// The is offline
        /// </summary>
        private bool isOffline;

        /// <summary>
        /// The step counter
        /// </summary>
        private int stepCounter;

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
            this.clientData = new StringBuilder();
            this.IsInitialized = false;
            this.IsOffline = false;
            this.InputQueue = new Queue<NetSimMessage>();
            this.Connections = new Dictionary<string, NetSimConnection>();
        }

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when client state is updated due routing or other events.
        /// </summary>
        public event Action StateUpdated
        {
            add
            {
                this.ClientStateUpdate += value;
            }

            remove
            {
                this.ClientStateUpdate -= value;
            }
        }

        /// <summary>
        /// Occurs when clientStateUpdate.
        /// </summary>
        private event Action ClientStateUpdate;

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        public Dictionary<string, NetSimConnection> Connections { get; set; }

        /// <summary>
        /// Gets the current data.
        /// </summary>
        /// <value>
        /// The current data.
        /// </value>
        public string CurrentData => this.clientData.ToString();

        /// <summary>
        /// Gets or sets the input queue.
        /// </summary>
        /// <value>
        /// The input queue.
        /// </value>
        public Queue<NetSimMessage> InputQueue { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is offline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        public bool IsOffline
        {
            get
            {
                return this.isOffline;
            }

            set
            {
                this.isOffline = value;

                if (!(this.Connections?.Values.Count > 0))
                {
                    return;
                }

                foreach (var connection in this.Connections.Values)
                {
                    connection.IsOffline = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the routing protocol.
        /// </summary>
        /// <value>
        /// The routing protocol.
        /// </value>
        public NetSimRoutingProtocol RoutingProtocol { get; set; }

        /// <summary>
        /// Gets the step counter.
        /// </summary>
        /// <value>
        /// The step counter.
        /// </value>
        public int StepCounter
        {
            get
            {
                return this.stepCounter;
            }

            private set
            {
                this.stepCounter = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Broadcasts the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="overrideSenderReceiver">if set to <c>true</c> [override sender receiver].</param>
        public void BroadcastMessage(NetSimMessage message, bool overrideSenderReceiver = true)
        {
            foreach (var connection in this.Connections)
            {
                // create copy of message
                NetSimMessage localCopy = (NetSimMessage)message.Clone();

                // insert receiver id 
                if (overrideSenderReceiver)
                {
                    localCopy.Receiver = connection.Key;
                    localCopy.Sender = this.Id;
                }

                // transport message
                connection.Value.StartTransportMessage(localCopy, this.Id, connection.Key);
            }
        }

        /// <summary>
        /// Initializes the protocol.
        /// </summary>
        /// <param name="protocolType">Type of the protocol.</param>
        public void InitializeProtocol(NetSimProtocolType protocolType)
        {
            // if client is already intialized - then this is a reset
            if (this.IsInitialized)
            {
                // clear pending messages in connections
                this.Connections.Values.ToList().ForEach(
                    c =>
                        {
                            c.PendingMessages.Clear();
                            c.TransmittedMessages.Clear();
                        });
            }

            // (re)set step counter
            this.StepCounter = 0;

            // (re)set the client data
            this.clientData = new StringBuilder();

            // create protocoll instance
            this.RoutingProtocol = RoutingProtocolFactory.CreateInstance(protocolType, this);

            // (re)set input queue
            this.InputQueue = new Queue<NetSimMessage>();

            // initialize protocol
            this.RoutingProtocol.Initialize();

            // client is initialized
            this.IsInitialized = true;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public void PerformSimulationStep()
        {
            this.RoutingProtocol.PerformRoutingStep();

            this.StepCounter++;
        }

        /// <summary>
        /// Receives a data message
        /// </summary>
        /// <param name="message">The message.</param>
        public void ReceiveData(NetSimMessage message)
        {
            this.clientData?.AppendLine(message.ToString());
        }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ReceiveMessage(NetSimMessage message)
        {
            this.InputQueue?.Enqueue(message);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(NetSimMessage message)
        {
            this.RoutingProtocol.SendMessage(message);
        }

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when the state is updated.
        /// </summary>
        protected void OnStateUpdated()
        {
            this.ClientStateUpdate?.Invoke();
        }
    }
}