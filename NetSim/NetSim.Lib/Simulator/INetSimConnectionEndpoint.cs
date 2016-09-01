using System;
using System.Linq;

using NetSim.Lib.Simulator.Components;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public interface INetSimConnectionEndpoint
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void ReceiveMessage(NetSimMessage message);

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        NetSimLocation Location { get; set; }

    }
}