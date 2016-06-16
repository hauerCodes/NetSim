using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using NetSim.Lib.Simulator;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimSimulator : IDrawableNetSimSimulator
    {
        /// <summary>
        /// Occurs when Updated.
        /// </summary>
        private event Action Updated;

        /// <summary>
        /// The step counter
        /// </summary>
        private int stepCounter;

        #region Constructor 

        public NetSimSimulator()
        {
            Clients = new List<NetSimClient>();
            Connections = new List<NetSimConnection>();
            this.stepCounter = 0;
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
        public int StepCounter => stepCounter;

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

            if(fromClient == null ||toClient == null)
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

            if(fromClient.Connections.ContainsKey(to) || toClient.Connections.ContainsKey(from))
            {
                return false;
            }

            Connections.Add(connection);
            fromClient.Connections.Add(to, connection);
            toClient.Connections.Add(from, connection);

            OnUpdated();

            return true;
        }

        /// <summary>
        /// Initializes the protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        public void InitializeProtocol(NetSimProtocolType protocol)
        {
            this.stepCounter = 0;

            foreach(var client in Clients)
            {
                client.InitializeProtocol(protocol);
            }
        }

        /// <summary>
        /// Performs the routing step.
        /// </summary>
        public void PerformRoutingStep()
        {
            //end the transmittion of messages started in the previous step
            EndTransmittingMessages();

            foreach (var client in Clients)
            {
                client.PerformRoutingStep();
            }

            stepCounter++;
        }

        /// <summary>
        /// Ends the transmitting messages.
        /// </summary>
        private void EndTransmittingMessages()
        {
            foreach(var connection in Connections.Where(c => c.IsTransmitting))
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

    }
}