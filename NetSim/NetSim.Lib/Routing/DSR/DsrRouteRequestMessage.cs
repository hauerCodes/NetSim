using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    /// <summary>
    /// The DSR Route Request Message
    /// This message is used to search route to the destination Id within this message.
    /// The routes gets stored in the nodes list along the found route.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.NetSimMessage" />
    public class DsrRouteRequestMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrRouteRequestMessage" /> class.
        /// </summary>
        public DsrRouteRequestMessage()
        {
            this.Nodes = new List<string>();
        }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public List<string> Nodes { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new DsrRouteRequestMessage()
            {
                Id = this.Id,
                RequestId = this.RequestId,
                Sender = this.Sender,
                Receiver = this.Receiver,
                Nodes = new List<string>(this.Nodes)
            };
        }
    }
}
