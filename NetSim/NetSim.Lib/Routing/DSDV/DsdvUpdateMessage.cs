using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSDV
{
    public class DsdvUpdateMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the update table.
        /// </summary>
        /// <value>
        /// The update table.
        /// </value>
        public DsdvTable UpdateTable { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new DsdvUpdateMessage()
            {
                UpdateTable = (DsdvTable)UpdateTable.Clone()
            };

            return CopyTo(clone);
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

            builder.AppendLine(base.ToString());
            builder.AppendLine($"| Dest Metric SeqNr");

            foreach (var entry in UpdateTable.Entries)
            {
                var dsdvEntry = (entry as DsdvTableEntry);

                if (dsdvEntry == null)
                {
                    continue;
                }

                builder.AppendLine($"| {dsdvEntry.Destination,4} {dsdvEntry.Metric,6} {dsdvEntry.SequenceNr,5}");
            }

            builder.AppendLine($"+[/{this.GetType().Name}]");

            return builder.ToString();
        }

    }
}
