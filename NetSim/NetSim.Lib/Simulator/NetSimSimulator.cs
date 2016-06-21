using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NetSim.Lib.Annotations;
using NetSim.Lib.Simulator;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimSimulator : IDrawableNetSimSimulator, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when Updated.
        /// </summary>
        private event Action Updated;

        /// <summary>
        /// The step counter
        /// </summary>
        private int stepCounter;

        /// <summary>
        /// The is initialized
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// The protocol
        /// </summary>
        private NetSimProtocolType protocol;

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimSimulator"/> class.
        /// </summary>
        public NetSimSimulator()
        {
            Clients = new List<NetSimClient>();
            Connections = new List<NetSimConnection>();
            StepCounter = 0;
            IsInitialized = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the simulator updates.
        /// </summary>
        public event Action SimulatorUpdated
        {
            add
            {
                Updated += value;
            }
            remove
            {
                Updated -= value;
            }
        }

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties 

        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        public List<NetSimClient> Clients { get; set; }

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        public List<NetSimConnection> Connections { get; set; }

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
            set
            {
                this.stepCounter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized
        {
            get
            {
                return isInitialized;
            }
            private set
            {
                isInitialized = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the protocol.
        /// </summary>
        /// <value>
        /// The protocol.
        /// </value>
        public NetSimProtocolType Protocol
        {
            get
            {
                return protocol;
            }
            set
            {
                this.protocol = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <returns></returns>
        public NetSimItem AddClient(string id, int left, int top)
        {
            var client = new NetSimClient(id, new NetSimLocation(left, top));

            // add client
            Clients.Add(client);

            // forward updates
            client.StateUpdated += OnUpdated;

            // if simulator already initialized
            if (this.IsInitialized)
            {
                // then intiailize new clients with same protocol
                client.InitializeProtocol(Protocol);
            }

            OnUpdated();

            return client;
        }

        /// <summary>
        /// Adds the connection.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        public bool AddConnection(string from, string to, int metric)
        {
            NetSimClient fromClient = Clients.FirstOrDefault(c => c.Id.Equals(from));
            NetSimClient toClient = Clients.FirstOrDefault(c => c.Id.Equals(to));

            if (fromClient == null || toClient == null)
            {
                return false;
            }

            NetSimConnection connection = new NetSimConnection()
            {
                EndPointA = fromClient,
                EndPointB = toClient,
                Id = $"{from} - {to}",
                Metric = metric
            };

            if (fromClient.Connections.ContainsKey(to) || toClient.Connections.ContainsKey(from))
            {
                return false;
            }

            // forward updates
            connection.StateUpdated += OnUpdated;

            Connections.Add(connection);
            fromClient.Connections.Add(to, connection);
            toClient.Connections.Add(from, connection);

            OnUpdated();

            return true;
        }

        /// <summary>
        /// Initializes the protocol.
        /// </summary>
        /// <param name="initProtocol">The initialize protocol.</param>
        public void InitializeProtocol(NetSimProtocolType initProtocol)
        {
            this.StepCounter = 0;
            this.Protocol = initProtocol;

            foreach (var client in Clients)
            {
                client.InitializeProtocol(initProtocol);
            }

            this.IsInitialized = true;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public void PerformSimulationStep()
        {
            //end the transmittion of messages started in the previous step
            EndTransmittingMessages();

            foreach (var client in Clients.OrderBy(x => Guid.NewGuid()))
            {
                client.PerformSimulationStep();
            }

            StepCounter++;
        }

        /// <summary>
        /// Ends the transmitting messages.
        /// </summary>
        private void EndTransmittingMessages()
        {
            foreach (var connection in Connections.Where(c => c.IsTransmitting))
            {
                connection.EndTransportMessages();
            }
        }

        /// <summary>
        /// Should be called when something to visualize gets updated.
        /// </summary>
        protected void OnUpdated()
        {
            Updated?.Invoke();
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}