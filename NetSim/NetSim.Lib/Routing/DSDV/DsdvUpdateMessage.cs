using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

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
            return new DsdvUpdateMessage()
            {
                Sender = this.Sender,
                Receiver = this.Receiver,
                UpdateTable = (DsdvTable)UpdateTable.Clone()
            };
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
            builder.AppendLine("--- Content ---");
            builder.AppendLine($"Dest Metric SeqNr");

            foreach (var entry in UpdateTable.Entries)
            {
                var dsdvEntry = (entry as DsdvTableEntry);

                if (dsdvEntry == null)
                {
                    continue;
                }

                builder.AppendLine($"{dsdvEntry.Destination,4} {dsdvEntry.Metric,6} {dsdvEntry.SequenceNr,5}");
            }

            return builder.ToString();
        }

    }
}
