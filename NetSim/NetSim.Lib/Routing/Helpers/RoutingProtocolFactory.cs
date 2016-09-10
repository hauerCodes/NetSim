// -----------------------------------------------------------------------
// <copyright file="RoutingProtocolFactory.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - RoutingProtocolFactory.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.Helpers
{
    using System;
    using System.Linq;
    using NetSim.Lib.Routing.AODV;
    using NetSim.Lib.Routing.DSDV;
    using NetSim.Lib.Routing.DSR;
    using NetSim.Lib.Routing.OLSR;
    using NetSim.Lib.Simulator;
    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The routing protocol factory.
    /// This class is used to create the routing protocol instances based on the protocol type.
    /// </summary>
    public static class RoutingProtocolFactory
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="protocolType">Type of the protocol.</param>
        /// <param name="client">The client.</param>
        /// <returns>The created routing protocol instance.</returns>
        public static NetSimRoutingProtocol CreateInstance(NetSimProtocolType protocolType, NetSimClient client)
        {
            switch (protocolType)
            {
                case NetSimProtocolType.AODV:
                    return new AodvRoutingProtocol(client);
                case NetSimProtocolType.DSDV:
                    return new DsdvRoutingProtocol(client);
                case NetSimProtocolType.DSR:
                    return new DsrRoutingProtocol(client);
                case NetSimProtocolType.OLSR:
                    return new OlsrRoutingProtocol(client);
            }

            return null;
        }
    }
}