// -----------------------------------------------------------------------
// <copyright file="DsdvTable.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsdvTable.cs</summary>
// -----------------------------------------------------------------------

// ReSharper disable ArrangeStaticMemberQualifier
namespace NetSim.Lib.Routing.DSDV
{
    using System;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsdv routing table implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTable" />
    public class DsdvTable : NetSimTable
    {
        /// <summary>
        /// Adds the route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public void AddRouteEntry(string destination, string nextHop, int metric, DsdvSequence sequenceNr)
        {
            this.Entries.Add(
                new DsdvTableEntry()
                {
                    Destination = destination,
                    NextHop = nextHop,
                    Metric = metric,
                    SequenceNr = sequenceNr,
                });
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned table instance.</returns>
        public override object Clone()
        {
            return new DsdvTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
        }

        /// <summary>
        /// Gets the route for.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns>The found route or null.</returns>
        public override NetSimTableEntry GetRouteFor(string destinationId)
        {
            return this.Entries.FirstOrDefault(e => e.Destination.Equals(destinationId));
        }

        /// <summary>
        /// Handles the update.
        /// </summary>
        /// <param name="senderId">The sender identifier.</param>
        /// <param name="receivedUpdate">The received update.</param>
        /// <returns>true if the table was updated; otherwise false</returns>
        public bool HandleUpdate(string senderId, DsdvTable receivedUpdate)
        {
            bool updated = false;

            // iterate through update 
            foreach (var updateRoute in receivedUpdate.Entries)
            {
                // search localroute
                var localRoute = this.Entries.FirstOrDefault(r => r.Destination.Equals(updateRoute.Destination));

                // ignore own local route (e.g. A A 0)
                if (localRoute != null && localRoute.Metric == 0)
                {
                    continue;
                }

                // if no local route exists add route with nexthop=sender and increment metric
                if (localRoute == null)
                {
                    var dsdvTableEntry = updateRoute as DsdvTableEntry;

                    if (dsdvTableEntry == null)
                    {
                        continue;
                    }

                    this.AddRouteEntry(
                        updateRoute.Destination,
                        senderId,
                        updateRoute.Metric + 1,
                        dsdvTableEntry.SequenceNr);
                    updated = true;
                }
                else
                {
                    // if route exists check if better metric and sequence number is higher (newer) than local number
                    var dsdvLocalRouteEntry = localRoute as DsdvTableEntry;
                    var dsdvUpdateRoute = updateRoute as DsdvTableEntry;

                    if (dsdvUpdateRoute == null || dsdvLocalRouteEntry == null)
                    {
                        continue;
                    }

                    var sequenceCompare = dsdvUpdateRoute.SequenceNr.CompareTo(dsdvLocalRouteEntry.SequenceNr);

                    switch (sequenceCompare)
                    {
                        case 0:

                            // if update route sequencenr is equal to local route sequencenr
                            if (updateRoute.Metric != NetSimTable.NotReachable)
                            {
                                // check if updateRoute (metric + 1) is better than local existant route
                                if (updateRoute.Metric + 1 < localRoute.Metric)
                                {
                                    // update and add increment metric
                                    dsdvLocalRouteEntry.NextHop = senderId;
                                    dsdvLocalRouteEntry.Metric = dsdvUpdateRoute.Metric + 1;
                                    dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();
                                    updated = true;
                                }
                            }

                            break;

                        case 1:

                            // if update route sequencenr is higher then local route sequencenr (update info is newer)

                            // if metric is notreachable
                            if (updateRoute.Metric == NetSimTable.NotReachable)
                            {
                                if (dsdvLocalRouteEntry.Metric != NetSimTable.NotReachable)
                                {
                                    // set local route not reachable
                                    dsdvLocalRouteEntry.Metric = NetSimTable.NotReachable;
                                    dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();

                                    // SetAllRoutesNotReachableForDisconnectedNextHop(dsdvLocalRouteEntry.NextHop);
                                    updated = true;
                                }
                            }
                            else
                            {
                                // if metric is the same 
                                if (updateRoute.Metric + 1 == localRoute.Metric)
                                {
                                    // update sequence nr and don' update hte route
                                    dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();
                                }
                                else
                                {
                                    // update local route with newer information (metric + 1)
                                    dsdvLocalRouteEntry.NextHop = senderId;
                                    dsdvLocalRouteEntry.Metric = dsdvUpdateRoute.Metric + 1;
                                    dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();
                                    updated = true;
                                }
                            }

                            break;
                    }
                }
            }

            return updated;
        }

        /// <summary>
        /// Sets all routes not reachable for next hop.
        /// </summary>
        /// <param name="nextHop">The next hop.</param>
        public void SetAllRoutesNotReachableForDisconnectedNextHop(string nextHop)
        {
            foreach (
                var netSimTableEntry in
                this.Entries.Where(e => e.NextHop.Equals(nextHop) && !e.Destination.Equals(e.NextHop)))
            {
                var entry = (DsdvTableEntry)netSimTableEntry;

                entry.Metric = NetSimTable.NotReachable;

                // increment sequence nr - outside of "destination" node
                entry.SequenceNr.SequenceNr++;
            }
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