// -----------------------------------------------------------------------
// <copyright file="OlsrTopologyTableEntry.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrTopologyTableEntry.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Linq;

    /// <summary>
    /// The olsr topology table entry implementation.
    /// </summary>
    /// <seealso cref="System.ICloneable" />
    public class OlsrTopologyTableEntry : ICloneable
    {
        /// <summary>
        /// Gets or sets the MPR selector identifier.
        /// </summary>
        /// <value>
        /// The MPR selector identifier.
        /// </value>
        public string MprSelectorId { get; set; }

        /// <summary>
        /// Gets or sets the originator identifier.
        /// </summary>
        /// <value>
        /// The originator identifier.
        /// </value>
        public string OriginatorId { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new OlsrTopologyTableEntry() { OriginatorId = this.OriginatorId, MprSelectorId = this.MprSelectorId };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.OriginatorId,10} {this.MprSelectorId}";
        }
    }
}