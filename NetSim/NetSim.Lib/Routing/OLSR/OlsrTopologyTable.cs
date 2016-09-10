// -----------------------------------------------------------------------
// <copyright file="OlsrTopologyTable.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrTopologyTable.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The olsr topology table implementation.
    /// </summary>
    /// <seealso cref="System.ICloneable" />
    public class OlsrTopologyTable : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrTopologyTable" /> class.
        /// </summary>
        public OlsrTopologyTable()
        {
            this.Entries = new List<OlsrTopologyTableEntry>();
        }

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<OlsrTopologyTableEntry> Entries { get; set; }

        /// <summary>
        /// Adds the entry.
        /// </summary>
        /// <param name="originatorId">The originator identifier.</param>
        /// <param name="mprSelectorId">The MPR selector identifier.</param>
        public void AddEntry(string originatorId, string mprSelectorId)
        {
            var entry = new OlsrTopologyTableEntry() { OriginatorId = originatorId, MprSelectorId = mprSelectorId };

            this.Entries.Add(entry);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new OlsrTopologyTable()
            {
                Entries = this.Entries.Select(e => (OlsrTopologyTableEntry)e.Clone()).ToList()
            };
        }

        /// <summary>
        /// Gets the entry for.
        /// </summary>
        /// <param name="searchId">The search identifier.</param>
        /// <returns>The the topology entry for the searched identifier or null.</returns>
        public OlsrTopologyTableEntry GetEntryFor(string searchId)
        {
            return this.Entries.FirstOrDefault(e => e.OriginatorId.Equals(searchId));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Originator MPR-Selector");
            foreach (var entry in this.Entries)
            {
                builder.AppendLine(entry.ToString());
            }

            return builder.ToString();
        }
    }
}