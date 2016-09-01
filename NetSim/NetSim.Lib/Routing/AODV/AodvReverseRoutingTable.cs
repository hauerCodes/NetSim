using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvReverseRoutingTable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrNeighborTable" /> class.
        /// </summary>
        public AodvReverseRoutingTable()
        {
            this.Entries = new List<AodvReverseRoutingTableEntry>();
        }

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<AodvReverseRoutingTableEntry> Entries { get; set; }

        /// <summary>
        /// Gets the entry for.
        /// </summary>
        /// <param name="searchId">The search identifier.</param>
        /// <returns></returns>
        public AodvReverseRoutingTableEntry GetEntryFor(string searchId)
        {
            return Entries.FirstOrDefault(e => e.NeighborId.Equals(searchId));
        }

        /// <summary>
        /// Adds the entry.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="accessThrough">The access though.</param>
        public void AddEntry(string sender, string accessThrough = null)
        {
            var entry = new AodvReverseRoutingTableEntry()
            {
                NeighborId = sender,
                IsMultiPointRelay = false
            };

            entry.AccessableThrough.Add(accessThrough);

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

            foreach (var entry in Entries)
            {
                builder.AppendLine(entry.ToString());
            }
            return builder.ToString();
        }

    }
}
