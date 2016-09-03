using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrToplogyTable : ICloneable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrNeighborTable"/> class.
        /// </summary>
        public OlsrToplogyTable()
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
        /// Gets the entry for.
        /// </summary>
        /// <param name="searchId">The search identifier.</param>
        /// <returns></returns>
        public OlsrTopologyTableEntry GetEntryFor(string searchId)
        {
            return Entries.FirstOrDefault(e => e.OrigniatorId.Equals(searchId));
        }

        /// <summary>
        /// Adds the entry.
        /// </summary>
        /// <param name="originatorId">The originator identifier.</param>
        /// <param name="mprSelectorId">The MPR selector identifier.</param>
        public void AddEntry(string originatorId, string mprSelectorId)
        {
            var entry = new OlsrTopologyTableEntry()
            {
                OrigniatorId = originatorId,
                MprSelectorId = mprSelectorId
            };

            this.Entries.Add(entry);
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
            foreach (var entry in Entries)
            {
                builder.AppendLine(entry.ToString());
            }
            return builder.ToString();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new OlsrToplogyTable() { Entries = this.Entries.Select(e => (OlsrTopologyTableEntry)e.Clone()).ToList() };
        }
    }
}
