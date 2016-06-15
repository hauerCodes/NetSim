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

        #region Constructor 

        public NetSimSimulator()
        {
            Clients = new List<NetSimClient>();
            Connections = new List<NetSimConnection>();
        }

        #endregion

        #region Events

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

        public List<NetSimClient> Clients { get; set; }

        public List<NetSimConnection> Connections { get; set; }

        #endregion

        public NetSimItem AddClient(string id, int left, int top)
        {
            var client = new NetSimClient(id, new NetSimLocation(left, top));

            Clients.Add(client);

            OnUpdated();

            return client;
        }

        public void AddConnection(string from, string to, int metric)
        {
            NetSimClient fromClient = Clients.FirstOrDefault(c => c.Id.Equals(from));
            NetSimClient toClient = Clients.FirstOrDefault(c => c.Id.Equals(to));

            if(fromClient == null ||toClient == null)
            {
                throw new ArgumentNullException();
            }

            NetSimConnection connection = new NetSimConnection()
            {
                From = fromClient,
                To = toClient,
                Id = $"{from} - {to}",
                Metric = metric
            };

            if(fromClient.Connections.ContainsKey(to) || toClient.Connections.ContainsKey(from))
            {
                throw new InvalidOperationException($"Connection between {from} and {to} already exsist!");
            }

            Connections.Add(connection);
            fromClient.Connections.Add(to, connection);
            toClient.Connections.Add(from, connection);

            OnUpdated();
        }

        public void InitializeProtocol(NetSimProtocol protocol)
        {
            foreach(var client in Clients)
            {
                client.InitializeProtocol(protocol);
            }
        }

        protected void OnUpdated()
        {
            Updated?.Invoke();
        }

    }
}