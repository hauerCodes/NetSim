// -----------------------------------------------------------------------
// <copyright file="SimulationStorage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - SimulationStorage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Storage
{
    using System;
    using System.IO;
    using System.Linq;
    using NetSim.Lib.Visualization;
    using Newtonsoft.Json;

    /// <summary>
    /// The simulation storage.
    /// Used for save and lode network information for the simulator.
    /// </summary>
    public class SimulationStorage
    {
        /// <summary>
        /// Loads the network.
        /// </summary>
        /// <param name="filepath">The file path.</param>
        /// <param name="creatorFunction">The creator function.</param>
        /// <returns>An simulator with a created network.</returns>
        public IDrawableNetSimSimulator LoadNetwork(
            string filepath,
            Func<StorageNetwork, IDrawableNetSimSimulator> creatorFunction)
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
        /// <param name="filepath">The file path.</param>
        /// <param name="simulator">The simulator.</param>
        public void SaveNetwork(string filepath, IDrawableNetSimSimulator simulator)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (StreamWriter sw = new StreamWriter(filepath))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this.CreateStorageNetwork(simulator));
                    writer.Flush();
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// Creates the storage network.
        /// </summary>
        /// <param name="simulator">The simulator.</param>
        /// <returns>The storage network based on the given simulator instance.</returns>
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