using System;
using System.Linq;
using System.Text;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvTable : NetSimTable
    {
        /// <summary>
        /// Adds the route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public void AddRouteEntry(string destination, string nextHop, int metric, AodvSequence sequenceNr)
        {
            this.Entries.Add(new AodvTableEntry()
            {
                Destination = destination,
                NextHop = nextHop,
                Metric = metric,
                SequenceNr = sequenceNr,
            });
        }

        /// <summary>
        /// Gets the route for.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns></returns>
        public override NetSimTableEntry GetRouteFor(string destinationId)
        {
            return Entries.FirstOrDefault(e => e.Destination.Equals(destinationId));
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new AodvTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
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

            builder.AppendLine($"Dest Next Metric SeqNr");
            builder.Append(base.ToString());

            return builder.ToString();
        }
    }
}