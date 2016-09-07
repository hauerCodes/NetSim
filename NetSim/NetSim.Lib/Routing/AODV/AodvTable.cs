using System;
using System.Collections.Generic;
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
        /// Handles the route maintaince.
        /// </summary>
        /// <param name="inactiveNeighbourId">The inactive neighbour identifier.</param>
        /// <returns>
        /// A List of Route Error Receivers
        /// </returns>
        public List<string> HandleRouteMaintaince(string inactiveNeighbourId)
        {
            List<string> routeErrorReceivers = new List<string>();

            // remove every route where inactive neighour is destination
            var directRoute = GetRouteFor(inactiveNeighbourId);

            if (directRoute != null)
            {
                // send rerr to every active neighbour of the route
                routeErrorReceivers.AddRange(GetActiveNeighbours(directRoute));

                Entries.Remove(directRoute);
            }

            // remove every route where a inactive neighbour is next hop
            var nextHopRoutes = Entries.Where(e => e.NextHop.Equals(inactiveNeighbourId)).ToList();

            foreach (var nextHopRoute in nextHopRoutes)
            {
                // send rerr to every active neighbour of the route
                routeErrorReceivers.AddRange(GetActiveNeighbours(nextHopRoute));

                Entries.Remove(nextHopRoute);
            }

            return routeErrorReceivers.Distinct().ToList();
        }

        /// <summary>
        /// Gets the active neighbours.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        private List<string> GetActiveNeighbours(NetSimTableEntry route)
        {
            var aodvroute = route as AodvTableEntry;

            return aodvroute?.ActiveNeighbours.Keys.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Handles the request route caching.
        /// </summary>
        /// <param name="reqMessage">The req message.</param>
        public void HandleRequestReverseRouteCaching(AodvRouteRequestMessage reqMessage)
        {
            // search for already cached route
            var route = (AodvTableEntry)this.GetRouteFor(reqMessage.Sender);

            // add route if route doesn't exist
            if (route == null)
            {
                // add route to table
                AddRouteEntry(reqMessage.Sender,
                    reqMessage.LastHop,
                    reqMessage.HopCount + 1,
                    (AodvSequence)reqMessage.SenderSequenceNr.Clone());
            }
            else
            {
                // if route exists check if sequencenr of request is newer - information in message is newer
                if (reqMessage.SenderSequenceNr.CompareTo(route.SequenceNr) == 1)
                {
                    //TODO check if needed
                    //if (reqMessage.HopCount + 1 < route.Metric)
                    //{

                        // remove route and add new one
                        Entries.Remove(route);

                        // add new route
                        AddRouteEntry(reqMessage.Sender,
                            reqMessage.LastHop,
                            reqMessage.HopCount + 1,
                            (AodvSequence)reqMessage.SenderSequenceNr.Clone());

                    //}
                }
            }
        }

        /// <summary>
        /// Handles the request route caching.
        /// </summary>
        /// <param name="repMessage">The req message.</param>
        public void HandleReplyRoute(AodvRouteReplyMessage repMessage)
        {
            // search for already cached route
            var route = (AodvTableEntry)this.GetRouteFor(repMessage.Sender);

            // add route if route doesn't exist
            if (route == null)
            {
                // add route to table
                AddRouteEntry(repMessage.Sender,
                    repMessage.LastHop,
                    repMessage.HopCount + 1,
                    (AodvSequence)repMessage.ReceiverSequenceNr.Clone());
            }
            else
            {
                // if route exists check if sequencenr of request is newer - information in message is newer
                if (repMessage.ReceiverSequenceNr.CompareTo(route.SequenceNr) == 1)
                {
                    if (repMessage.HopCount + 1 < route.Metric)
                    {
                        // remove route and add new one
                        Entries.Remove(route);

                        // add new route
                        AddRouteEntry(repMessage.Sender,
                            repMessage.LastHop,
                            repMessage.HopCount + 1,
                            (AodvSequence)repMessage.ReceiverSequenceNr.Clone());
                    }
                }
            }

            // handle active neighbour list of route
            route = (AodvTableEntry)this.GetRouteFor(repMessage.Sender);

            // add repmessage sender to active neighbours of route
            if (!route.ActiveNeighbours.ContainsKey(repMessage.Sender))
            {
                route.ActiveNeighbours.Add(repMessage.Sender, 0);
            }

            // add repmessage last hop to active neighbours of route
            if (!route.ActiveNeighbours.ContainsKey(repMessage.LastHop))
            {
                route.ActiveNeighbours.Add(repMessage.LastHop, 0);
            }
        }

        /// <summary>
        /// Searches the cached route.
        /// </summary>
        /// <param name="reqMessage">The req message.</param>
        /// <returns></returns>
        public AodvTableEntry SearchCachedRoute(AodvRouteRequestMessage reqMessage)
        {
            // Check if route was cached
            var searchRoute = GetRouteFor(reqMessage.Receiver);

            if (searchRoute == null)
            {
                return null;
            }

            // convert to right table entry
            var aodvRoute = (AodvTableEntry)searchRoute;

            // add back route to table  with data from rreq
            AddRouteEntry(reqMessage.Sender,
                reqMessage.LastHop,
                reqMessage.HopCount,
                (AodvSequence)reqMessage.SenderSequenceNr.Clone());

            return aodvRoute;
        }

        /// <summary>
        /// Adds the active neigbour.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="activeNeighbour">The active neighbour.</param>
        public void AddActiveNeigbour(string destination, string activeNeighbour)
        {
            var route =GetRouteFor(destination) as AodvTableEntry;

            if (route == null)
            {
                return;
            }

            if (!route.ActiveNeighbours.ContainsKey(activeNeighbour))
            {
                route.ActiveNeighbours.Add(activeNeighbour, 0);
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

            builder.AppendLine($"Dest Next Metric SeqNr Neighbours");
            builder.Append(base.ToString());

            return builder.ToString();
        }

       
    }
}