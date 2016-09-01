using System;
using System.Linq;
using System.Text;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

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
            foreach (var updateRoute in receivedUpdate.Entries)
            {
                // search localroute
                var localRoute = Entries.FirstOrDefault(r => r.Destination.Equals(updateRoute.Destination));

                //ignore own local route
                if (localRoute != null && localRoute.Metric == 0)
                {
                    continue;
                }

                // if no local route exists add route with nexthop sender and increment metric
                if (localRoute == null)
                {
                    var dsdvTableEntry = updateRoute as DsdvTableEntry;

                    if (dsdvTableEntry == null) continue;
                    AddRouteEntry(
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

                    if (dsdvUpdateRoute == null || dsdvLocalRouteEntry == null) continue;

                    var sequenceCompare = dsdvUpdateRoute.SequenceNr.CompareTo(dsdvLocalRouteEntry.SequenceNr);

                    switch (sequenceCompare)
                    {
                        case 0: // if update route sequencenr is equal to local route sequencenr

                            // check if updateRoute (metric + 1) is better than local existant route
                            if (updateRoute.Metric + 1 < localRoute.Metric)
                            {
                                // update and add increment metric
                                dsdvLocalRouteEntry.NextHop = senderId;
                                dsdvLocalRouteEntry.Metric = dsdvUpdateRoute.Metric + 1;
                                dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();
                                updated = true;
                            }

                            break;
                        case 1: // if update route sequencenr is higher then local route sequencenr

                            // if metric is the same 
                            if (updateRoute.Metric + 1 == localRoute.Metric)
                            {
                                // update sequence nr
                                dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();
                            }
                            else
                            {
                                // update local route with newer information (metric + 1)
                                dsdvLocalRouteEntry.NextHop = senderId;

                                if (dsdvUpdateRoute.Metric != NotReachable)
                                {
                                    dsdvLocalRouteEntry.Metric = dsdvUpdateRoute.Metric + 1;
                                }
                                else
                                {
                                    dsdvLocalRouteEntry.Metric = dsdvUpdateRoute.Metric;

                                    SetAllRoutesNotReachableForDisconnectedNextHop(dsdvLocalRouteEntry.NextHop);

                                }
                                dsdvLocalRouteEntry.SequenceNr = (DsdvSequence)dsdvUpdateRoute.SequenceNr.Clone();
                                updated = true;
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
            foreach(var netSimTableEntry in Entries.Where(e => e.NextHop.Equals(nextHop) && !e.Destination.Equals(e.NextHop)))
            {
                var entry = (DsdvTableEntry)netSimTableEntry;

                entry.Metric = NotReachable;
                //increment sequence nr - outside of "destination" node
                entry.SequenceNr.SequenceNr++;

            }
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