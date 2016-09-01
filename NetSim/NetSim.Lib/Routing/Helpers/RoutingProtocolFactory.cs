using System;
using System.Linq;

using NetSim.Lib.Routing.AODV;
using NetSim.Lib.Routing.DSDV;
using NetSim.Lib.Routing.DSR;
using NetSim.Lib.Routing.OLSR;
using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.Helpers
{
    public static class RoutingProtocolFactory
    {
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="protocolType">Type of the protocol.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
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
