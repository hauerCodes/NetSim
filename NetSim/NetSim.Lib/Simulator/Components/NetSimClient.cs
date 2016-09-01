using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NetSim.Lib.Annotations;
using NetSim.Lib.Routing;
using NetSim.Lib.Routing.Helpers;
using NetSim.Lib.Simulator.Messages;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator.Components
{
    public class NetSimClient : NetSimItem, INotifyPropertyChanged, INetSimConnectionEndpoint
    {
        /// <summary>
        /// Occurs when clientStateUpdate.
        /// </summary>
        private event Action ClientStateUpdate;

        /// <summary>
        /// The step counter
        /// </summary>
        private int stepCounter;

        /// <summary>
        /// The client data
        /// </summary>
        private StringBuilder clientData;

        /// <summary>
        /// The is offline
        /// </summary>
        private bool isOffline;

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
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        public Dictionary<string, NetSimConnection> Connections { get; set; }

        /// <summary>
        /// Gets or sets the input queue.
        /// </summary>
        /// <value>
        /// The input queue.
        /// </value>
        public Queue<NetSimMessage> InputQueue { get; set; }

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
                return stepCounter;
            }
            private set
            {
                this.stepCounter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is offline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        public bool IsOffline
        {
            get
            {
                return isOffline;
            }
            set
            {
                isOffline = value;

                if(!(Connections?.Values?.Count > 0))
                {
                    return;
                }

                foreach (var connection in Connections.Values)
                {
                    connection.IsOffline = value;
                }
            }
        }

        /// <summary>
        /// Gets the current data.
        /// </summary>
        /// <value>
        /// The current data.
        /// </value>
        public string CurrentData => clientData.ToString();

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

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes the protocol.
        /// </summary>
        /// <param name="protocolType">Type of the protocol.</param>
        public void InitializeProtocol(NetSimProtocolType protocolType)
        {
            // if client is already intialized - then this is a reset
            if (IsInitialized)
            {
                //clear pending messages in connections
                Connections.Values.ToList().ForEach(c =>
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

            //initialize protocol
            RoutingProtocol.Initialize();

            // client is initialized
            this.IsInitialized = true;
        }

        /// <summary>
        /// Broadcasts the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="overrideSenderReceiver">if set to <c>true</c> [override sender receiver].</param>
        public void BroadcastMessage(NetSimMessage message, bool overrideSenderReceiver = true)
        {
            foreach (var connection in Connections)
            {
                //create copy of message
                NetSimMessage localCopy = (NetSimMessage)message.Clone();

                // insert receiver id 
                if (overrideSenderReceiver)
                {
                    localCopy.Receiver = connection.Key;
                    localCopy.Sender = Id;
                }

                //TODO
                // reset transmission step - because this message gets forwarded 
                //localCopy.TransmissionStep = NetSimMessageTransmissionStep.Sending;
                //localCopy.NextReceiver = connection.Key;

                //transport message
                connection.Value.StartTransportMessage(localCopy, this.Id, connection.Key);
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(NetSimMessage message)
        {
            RoutingProtocol.SendMessage(message);
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
        /// Receives a datamessage
        /// </summary>
        /// <param name="message">The message.</param>
        public void ReceiveData(NetSimMessage message)
        {
            clientData?.AppendLine(message.ToString());
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public void PerformSimulationStep()
        {
            RoutingProtocol.PerformRoutingStep();

            StepCounter++;
        }

        /// <summary>
        /// Called when the state is updated.
        /// </summary>
        protected void OnStateUpdated()
        {
            ClientStateUpdate?.Invoke();
        }

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}