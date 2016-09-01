using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Visualization;

using Newtonsoft.Json;

namespace NetSim.Lib.Storage
{
    public class SimulationStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationStorage"/> class.
        /// </summary>
        public SimulationStorage() { }

        /// <summary>
        /// Loads the network.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="creatorFunction">The creator function.</param>
        /// <returns></returns>
        public IDrawableNetSimSimulator LoadNetwork(string filepath, Func<StorageNetwork, IDrawableNetSimSimulator> creatorFunction)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader sr = new StreamReader(filepath))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var network = serializer.Deserialize<StorageNetwork>(reader);
                    return creatorFunction(network);
                }
            }
        }

        /// <summary>
        /// Saves the network.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="simulator">The simulator.</param>
        public void SaveNetwork(string filepath, IDrawableNetSimSimulator simulator)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (StreamWriter sw = new StreamWriter(filepath))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, CreateStorageNetwork(simulator));
                    writer.Flush();
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// Creates the storage network.
        /// </summary>
        /// <param name="simulator">The simulator.</param>
        /// <returns></returns>
        private StorageNetwork CreateStorageNetwork(IDrawableNetSimSimulator simulator)
        {
            var network = new StorageNetwork
            {
                Clients =
                    simulator.Clients.Select(
                        c =>
                        new StorageClient()
                        {
                            Id = c.Id,
                            IsOffline = c.IsOffline,
                            Left = c.Location.Left,
                            Top = c.Location.Top
                        }).ToArray(),
                Connections =
                    simulator.Connections.Select(
                        c =>
                        new StorageConnection()
                        {
                            EndpointA = c.EndPointA.Id,
                            EndpointB = c.EndPointB.Id,
                            Id = c.Id,
                            IsOffline = c.IsOffline,
                            Metric = c.Metric
                        }).ToArray()
            };

            return network;
        }

    }
}
