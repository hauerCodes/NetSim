// -----------------------------------------------------------------------
// <copyright file="StorageConnection.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - StorageConnection.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Storage
{
    using System;

    /// <summary>
    /// The storage connection is a representation of a connection for saving.
    /// </summary>
    [Serializable]
    public class StorageConnection
    {
        /// <summary>
        /// Gets or sets the endpoint a.
        /// </summary>
        /// <value>
        /// The endpoint a.
        /// </value>
        public string EndpointA { get; set; }

        /// <summary>
        /// Gets or sets the endpoint b.
        /// </summary>
        /// <value>
        /// The endpoint b.
        /// </value>
        public string EndpointB { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is offline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        public bool IsOffline { get; set; }

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public int Metric { get; set; }
    }
}