using System;
using System.Linq;
using System.Text;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSDV
{
    public class DsdvTable : NetSimTable
    {
        /// <summary>
        /// Adds the initial route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public void AddInitialRouteEntry(string destination, string nextHop, int metric, DsdvSequence sequenceNr)
        {
            this.Entries.Add(new DsdvTableEntry()
            {
                Destination = destination,
                NextHop = nextHop,
                Metric = metric,
                SequenceNr = sequenceNr
            });
        }

        /// <summary>
        /// Adds the route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public void AddRouteEntry(string destination, string nextHop, int metric, DsdvSequence sequenceNr)
        {
            this.Entries.Add(new DsdvTableEntry()
            {
                Destination = destination,
                NextHop = nextHop,
                Metric = metric,
                SequenceNr = sequenceNr,
            });
        }

        /// <summary>
        /// Handles the update.
        /// </summary>
        /// <param name="senderId">The sender identifier.</param>
        /// <param name="receivedUpdate">The received update.</param>
        /// <returns></returns>
        public bool HandleUpdate(string senderId, DsdvTable receivedUpdate)
        {
            bool updated = false;

            // iterate through update 
            foreach(var updateRoute in receivedUpdate.Entries)
            {
                // search localroute
                var localRoute = Entries.FirstOrDefault(r => r.Destination.Equals(updateRoute.Destination));

                // if no local route exists add route with nexthop sender and increment metric
                if (localRoute == null)
                {
                    var dsdvTableEntry = updateRoute as DsdvTableEntry;

                    if (dsdvTableEntry != null)
                    {
                        AddRouteEntry(
                            updateRoute.Destination,
                            senderId,
                            updateRoute.Metric + 1,
                            dsdvTableEntry.SequenceNr);
                        updated = true;
                    }
                }
                else
                {
                    // if updateRoute (metric + 1) is better than local existant route - update and add increment metric
                    if (updateRoute.Metric + 1 < localRoute.Metric)
                    {

                        updated = true;
                    }
                }
            }

            return updated;
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
            return new DsdvTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
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