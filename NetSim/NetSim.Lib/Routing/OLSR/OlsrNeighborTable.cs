using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrNeighborTable
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrNeighborTable"/> class.
        /// </summary>
        public OlsrNeighborTable()
        {
            this.Entries = new List<OlsrNeighborTableEntry>();
        }

        

        
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<OlsrNeighborTableEntry> Entries { get; set; }

        

        /// <summary>
        /// Gets the entry for.
        /// </summary>
        /// <param name="searchId">The search identifier.</param>
        /// <returns></returns>
        public OlsrNeighborTableEntry GetEntryFor(string searchId)
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
            var entry = new OlsrNeighborTableEntry()
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
