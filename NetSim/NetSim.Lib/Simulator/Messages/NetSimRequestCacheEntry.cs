using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSim.Lib.Simulator.Messages
{
    public class NetSimRequestCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimRequestCacheEntry"/> class.
        /// </summary>
        public NetSimRequestCacheEntry()
        {
            this.ChachedRequests = new List<int>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the chached requests.
        /// </summary>
        /// <value>
        /// The chached requests.
        /// </value>
        public List<int> ChachedRequests { get; set; }

        /// <summary>
        /// Determines whether this entry has chached request with the specified identifier.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        public bool HasCachedRequest(int requestId)
        {
            return ChachedRequests.Contains(requestId);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Id} {String.Join(",", ChachedRequests)}";
        }
    }
}
