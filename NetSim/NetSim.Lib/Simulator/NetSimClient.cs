using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NetSim.Lib.Annotations;
using NetSim.Lib.Routing;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
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
            this.clientData = new StringBuilder();
            this.IsInitialized = false;
            this.InputQueue = new Queue<NetSimMessage>();
            this.Connections = new Dictionary<string, NetSimConnection>();
        }

        #endregion

        #region Properties

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
        /// Gets the current data.
        /// </summary>
        /// <value>
        /// The current data.
        /// </value>
        public string CurrentData => clientData.ToString();

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

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Initializes the protocol.
        /// </summary>
        /// <param name="protocolType">Type of the protocol.</param>
        public void InitializeProtocol(NetSimProtocolType protocolType)
        {
            // if client is already intialized - then this is a reset
            if(IsInitialized)
            {
                //clear pending messages in connections
                Connections.Values.ToList().ForEach(c => { c.PendingMessages.Clear(); });
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
        public void BroadcastMessage(NetSimMessage message)
        {
            foreach(var connection in Connections)
            {
                //create copy of message
                NetSimMessage localCopy = (NetSimMessage)message.Clone();
                
                // insert receiver id 
                localCopy.Receiver = connection.Key;
                localCopy.NextReceiver = connection.Key;
                localCopy.Sender = Id;

                //transport message
                connection.Value.StartTransportMessage(localCopy);
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(NetSimMessage message)
        {
            string nextHopId = RoutingProtocol.GetRoute(message.Receiver);

            //"hack" to determine the receiver endpoint of message
            message.NextReceiver = nextHopId;

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