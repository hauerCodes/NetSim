// -----------------------------------------------------------------------
// <copyright file="DsrTable.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - DsrTable.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.DSR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The dsr table implementation.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimTable" />
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
            this.Entries.Add(
                new DsrTableEntry()
                {
                    Destination = destination,
                    Metric = metric,
                    Route = new List<string>() { nextHop }
                });
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public override object Clone()
        {
            return new DsrTable() { Entries = this.Entries.Select(e => (NetSimTableEntry)e.Clone()).ToList() };
        }

        /// <summary>
        /// Gets the route for.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns>
        /// The found route for the destination or null.
        /// </returns>
        public override NetSimTableEntry GetRouteFor(string destinationId)
        {
            return this.Entries.FirstOrDefault(e => e.Destination.Equals(destinationId));
        }

        /// <summary>
        /// Handles the response.
        /// </summary>
        /// <param name="endpointFrom">The endpointFrom.</param>
        /// <param name="notReachableNodeTo">The not reachable node.</param>
        public void HandleError(string endpointFrom, string notReachableNodeTo)
        {
            // search entries where message endpointFrom and message notreachable are in path (direct connected)
            foreach (
                var netSimTableEntry in
                this.Entries.Where(e => ((DsrTableEntry)e).Route.Contains(notReachableNodeTo)).ToList())
            {
                var entry = (DsrTableEntry)netSimTableEntry;

                // search the index of  the notreachable node in route
                int index = entry.Route.IndexOf(notReachableNodeTo);

                // if index - 1 out or range - endpoint from is not in route
                if (index - 1 < 0)
                {
                    continue;
                }

                // if route element before the notreachable node equals the sender of the error message 
                if (entry.Route[index - 1].Equals(endpointFrom))
                {
                    // remove the route 
                    this.RemoveRoute(entry.Destination);
                }
            }
        }

        /// <summary>
        /// Handles the request route caching.
        /// </summary>
        /// <param name="repMessage">The rep message.</param>
        /// <param name="clientId">The client identifier.</param>
        public void HandleReplyRouteCaching(DsrRouteReplyMessage repMessage, string clientId)
        {
            // search for already cached route
            var route = (DsrTableEntry)this.GetRouteFor(repMessage.Sender);

            // skip the route until the own id is found in it - the rest is the path to the endpointFrom
            var cachedRoute = repMessage.Route.SkipWhile(e => !e.Equals(clientId)).ToList();

            // if route is empty drop
            if (cachedRoute.Count == 0)
            {
                return;
            }

            if (route == null)
            {
                // add route to table
                this.Entries.Add(
                    new DsrTableEntry()
                    {
                        Destination = repMessage.Sender,
                        Route = cachedRoute,
                        Metric = cachedRoute.Count - 1
                    });
            }
            else
            {
                // check if new route is shorter
                if (cachedRoute.Count - 1 < route.Metric)
                {
                    // remove route and add new one
                    this.Entries.Remove(route);

                    // add new route
                    this.Entries.Add(
                        new DsrTableEntry()
                        {
                            Destination = repMessage.Sender,
                            Route = cachedRoute,
                            Metric = cachedRoute.Count - 1
                        });
                }
            }
        }

        /// <summary>
        /// Handles the request route caching.
        /// </summary>
        /// <param name="reqMessage">The request message.</param>
        public void HandleRequestRouteCaching(DsrRouteRequestMessage reqMessage)
        {
            // search for already cached route
            var route = (DsrTableEntry)this.GetRouteFor(reqMessage.Sender);

            // reverse the request route
            var cachedRoute = reqMessage.Nodes.Reverse<string>().ToList();

            if (route == null)
            {
                // add route to table
                this.Entries.Add(
                    new DsrTableEntry()
                    {
                        Destination = reqMessage.Sender,
                        Route = cachedRoute,
                        Metric = cachedRoute.Count - 1
                    });
            }
            else
            {
                // check if new route is shorter
                if (cachedRoute.Count < route.Metric)
                {
                    // remove route and add new one
                    this.Entries.Remove(route);

                    // add new route
                    this.Entries.Add(
                        new DsrTableEntry()
                        {
                            Destination = reqMessage.Sender,
                            Route = cachedRoute,
                            Metric = cachedRoute.Count - 1
                        });
                }
            }
        }

        /// <summary>
        /// Handles the response.
        /// </summary>
        /// <param name="message">The message.</param>
        public void HandleResponse(DsrRouteReplyMessage message)
        {
            // search for a route for this destination
            var entry = this.GetRouteFor(message.Sender);

            // if no route found or the metric of the found route is bigger
            if (entry == null || entry.Metric > message.Route.Count - 1)
            {
                this.Entries.Add(
                    new DsrTableEntry()
                    {
                        Destination = message.Sender,
                        Metric = message.Route.Count - 1,
                        Route = new List<string>(message.Route)
                    });
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

            builder.AppendLine($"Dest Metric Route");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        /// <summary>
        /// Removes the route.
        /// </summary>
        /// <param name="destination">The destination.</param>
        private void RemoveRoute(string destination)
        {
            var entry = this.GetRouteFor(destination);

            if (entry != null)
            {
                this.Entries.Remove(entry);
            }
        }
    }
}