using System;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvTable : NetSimTable
    {
        /// <summary>
        /// Adds the initial route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public void AddInitialRouteEntry(string destination, string nextHop, int metric, AodvSequence sequenceNr)
        {
            this.Entries.Add(new AodvTableEntry()
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

       
    }
}