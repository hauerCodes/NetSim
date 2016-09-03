using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrTopologyTableEntry : ICloneable
    {

        /// <summary>
        /// Gets or sets the origniator identifier.
        /// </summary>
        /// <value>
        /// The origniator identifier.
        /// </value>
        public string OrigniatorId { get; set; }

        /// <summary>
        /// Gets or sets the MPR selector identifier.
        /// </summary>
        /// <value>
        /// The MPR selector identifier.
        /// </value>
        public string MprSelectorId { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{OrigniatorId,10} {MprSelectorId}";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new OlsrTopologyTableEntry()
            {
                OrigniatorId = this.OrigniatorId,
                MprSelectorId = this.MprSelectorId
            };
        }
    }
}