// -----------------------------------------------------------------------
// <copyright file="INetSimConnectionEndpoint.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - INetSimConnectionEndpoint.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Visualization;

    /// <summary>
    /// The connection endpoint interface.
    /// This interface is used to describe an message receiver endpoint.
    /// </summary>
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
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        NetSimLocation Location { get; set; }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void ReceiveMessage(NetSimMessage message);
    }
}