// -----------------------------------------------------------------------
// <copyright file="NetSimRequestCacheEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimRequestCacheEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The request cache entry implementation is used for saving cached route requests.
    /// </summary>
    public class NetSimRequestCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimRequestCacheEntry"/> class.
        /// </summary>
        public NetSimRequestCacheEntry()
        {
            this.CachedRequests = new List<int>();
        }

        /// <summary>
        /// Gets or sets the cached requests.
        /// </summary>
        /// <value>
        /// The cached requests.
        /// </value>
        public List<int> CachedRequests { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Determines whether this entry has cached request with the specified identifier.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified request identifier is  cached request; otherwise, <c>false</c>.
        /// </returns>
        public bool HasCachedRequest(int requestId)
        {
            return this.CachedRequests.Contains(requestId);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.Id} {string.Join(",", this.CachedRequests)}";
        }
    }
}