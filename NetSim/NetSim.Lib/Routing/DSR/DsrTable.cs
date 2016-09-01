using System;
using System.Collections.Generic;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrTable : NetSimTable
    {
        /// <summary>
        /// Adds the initial route entry.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="nextHop">The next hop.</param>
        /// <param name="metric">The metric.</param>
        public void AddInitialRouteEntry(string destination, string nextHop, int metric)
        {
            this.Entries.Add(new DsrTableEntry()
            {
                Destination = destination,
                Metric = metric,
                //NextHop = nextHop,
                Route = new List<string>() { nextHop }
            });
        }

        /// <summary>
        /// Handles the response.
        /// </summary>
        /// <param name="message">The message.</param>
        public void HandleResponse(DsrRouteResponseMessage message)
        {
            // search for a route for this destination
            var entry = GetRouteFor(message.Sender);

            // if no route found or the metric of the found route is bigger
            if(entry == null || entry.Metric > message.Route.Count)
            {
                this.Entries.Add(new DsrTableEntry()
                {
                    Destination = message.Sender,
                    Metric = message.Route.Count,
                    Route = new List<string>(message.Route)
                });
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
            return new DsrTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
        }

    }
}