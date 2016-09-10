// -----------------------------------------------------------------------
// <copyright file="DsdvUpdateMessage.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsdvUpdateMessage.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSDV
{
    using System;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsdv update message.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimMessage" />
    public class DsdvUpdateMessage : NetSimMessage
    {
        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "Update";

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
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            var clone = new DsdvUpdateMessage() { UpdateTable = (DsdvTable)this.UpdateTable.Clone() };

            return this.CopyTo(clone);
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

            foreach (var entry in this.UpdateTable.Entries)
            {
                var dsdvEntry = entry as DsdvTableEntry;

                if (dsdvEntry == null)
                {
                    continue;
                }

                builder.AppendLine($"| {dsdvEntry.Destination,4} {dsdvEntry.Metric,6} {dsdvEntry.SequenceNr,5}");
            }

            builder.Append($"+[/{this.GetType().Name}]");

            return builder.ToString();
        }
    }
}