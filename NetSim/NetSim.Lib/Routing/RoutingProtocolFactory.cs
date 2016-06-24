using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Routing.AODV;
using NetSim.Lib.Routing.DSDV;
using NetSim.Lib.Routing.DSR;
using NetSim.Lib.Routing.OLSR;
using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing
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
