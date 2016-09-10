// -----------------------------------------------------------------------
// <copyright file="NetSimSimulator.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimSimulator.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using NetSim.Lib.Annotations;
    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Visualization;

    /// <summary>
    /// The simulator implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Visualization.IDrawableNetSimSimulator" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class NetSimSimulator : IDrawableNetSimSimulator, INotifyPropertyChanged
    {
        /// <summary>
        /// The is initialized
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// The protocol
        /// </summary>
        private NetSimProtocolType protocol;

        /// <summary>
        /// The step counter
        /// </summary>
        private int stepCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimSimulator"/> class.
        /// </summary>
        public NetSimSimulator()
        {
            this.Clients = new List<NetSimClient>();
            this.Connections = new List<NetSimConnection>();
            this.StepCounter = 0;
            this.IsInitialized = false;
        }

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the simulator updates.
        /// </summary>
        public event Action SimulatorUpdated
        {
            add
            {
                this.Updated += value;
            }

            remove
            {
                this.Updated -= value;
            }
        }

        /// <summary>
        /// Occurs when Updated.
        /// </summary>
        private event Action Updated;

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
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized
        {
            get
            {
                return this.isInitialized;
            }

            private set
            {
                this.isInitialized = value;
                this.OnPropertyChanged();
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
                return this.protocol;
            }

            set
            {
                this.protocol = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the step counter.
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

            set
            {
                this.stepCounter = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <returns>The created client.</returns>
        public NetSimItem AddClient(string id, int left, int top)
        {
            var client = new NetSimClient(id, new NetSimLocation(left, top));

            // add client
            this.Clients.Add(client);

            // forward updates
            client.StateUpdated += this.OnUpdated;

            // if simulator already initialized
            if (this.IsInitialized)
            {
                // then intiailize new clients with same protocol
                client.InitializeProtocol(this.Protocol);
            }

            this.OnUpdated();

            return client;
        }

        /// <summary>
        /// Adds the connection.
        /// </summary>
        /// <param name="from">From parameter.</param>
        /// <param name="to">To parameter.</param>
        /// <param name="metric">The metric.</param>
        /// <returns>true if the connection was created; otherwise false.</returns>
        public bool AddConnection(string from, string to, int metric)
        {
            NetSimClient fromClient = this.Clients.FirstOrDefault(c => c.Id.Equals(from));
            NetSimClient toClient = this.Clients.FirstOrDefault(c => c.Id.Equals(to));

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
            connection.StateUpdated += this.OnUpdated;

            this.Connections.Add(connection);
            fromClient.Connections.Add(to, connection);
            toClient.Connections.Add(from, connection);

            this.OnUpdated();

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

            foreach (var client in this.Clients)
            {
                client.InitializeProtocol(initProtocol);
            }

            this.OnUpdated();
            this.IsInitialized = true;
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public void PerformSimulationStep()
        {
            // if connection has pending mesasges to deliver - do this first
            if (this.Connections.Any(c => c.IsTransmitting))
            {
                // end the transmittion of messages started in the previous step
                this.EndTransmittingMessages();
            }
            else
            {
                // otherwise perform simulation steps
                foreach (var client in this.Clients.OrderBy(x => Guid.NewGuid()))
                {
                    client.PerformSimulationStep();
                }
            }

            this.StepCounter++;
        }

        /// <summary>
        /// Removes the client.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>true if the client was removed; otherwise false.</returns>
        public bool RemoveClient(string id)
        {
            var delClient = this.Clients.FirstOrDefault(c => c.Id.Equals(id));

            if (delClient == null)
            {
                return false;
            }

            var connectionListCopy = delClient.Connections.Values.ToList();

            // delete all connections from and to client
            foreach (var delconnection in connectionListCopy)
            {
                this.RemoveConnection(delconnection.EndPointA.Id, delconnection.EndPointB.Id);
            }

            // delete the client
            this.Clients.Remove(delClient);

            return true;
        }

        /// <summary>
        /// Removes the connection.
        /// EndPointA and B can be switched!
        /// </summary>
        /// <param name="endPointA">The end point a.</param>
        /// <param name="endPointB">The end point b.</param>
        /// <returns>true if the connection was removed; otherwise false.</returns>
        public bool RemoveConnection(string endPointA, string endPointB)
        {
            NetSimClient endPointAClient = this.Clients.FirstOrDefault(c => c.Id.Equals(endPointA));
            NetSimClient endPointBClient = this.Clients.FirstOrDefault(c => c.Id.Equals(endPointB));

            if (endPointAClient == null || endPointBClient == null)
            {
                return false;
            }

            var delConnection =
                this.Connections.FirstOrDefault(
                    c => c.EndPointA.Id.Equals(endPointA) && c.EndPointB.Id.Equals(endPointB))
                ?? this.Connections.FirstOrDefault(
                    c => c.EndPointA.Id.Equals(endPointB) && c.EndPointB.Id.Equals(endPointA));

            if (delConnection == null)
            {
                return false;
            }

            endPointAClient.Connections.Remove(endPointB);
            endPointBClient.Connections.Remove(endPointA);
            this.Connections.Remove(delConnection);

            return true;
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Should be called when something to visualize gets updated.
        /// </summary>
        protected void OnUpdated()
        {
            this.Updated?.Invoke();
        }

        /// <summary>
        /// Ends the transmitting messages.
        /// </summary>
        private void EndTransmittingMessages()
        {
            foreach (var connection in this.Connections)
            {
                connection.EndTransportMessages();
            }
        }
    }
}