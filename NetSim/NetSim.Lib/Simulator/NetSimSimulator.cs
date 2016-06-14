﻿using System;
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
        /// Occurs when updated.
        /// </summary>
        private event Action updated;

        public NetSimSimulator()
        {
            Clients = new List<NetSimClient>();
            Connections = new List<NetSimConnection>();
        }

        public event Action Updated
        {
            add
            {
                updated += value;
            }
            remove
            {
                updated -= value;
            }
        }

        public List<NetSimClient> Clients { get; set; }

        public List<NetSimConnection> Connections { get; set; }

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

        protected void OnUpdated()
        {
            updated?.Invoke();
        }

    }
}