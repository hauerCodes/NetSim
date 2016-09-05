using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSR
{
    /// <summary>
    /// The DSR Route Request Message
    /// This message is used to search route to the destination Id within this message.
    /// The routes gets stored in the nodes list along the found route.
    /// </summary>
    /// <seealso cref="NetSimMessage" />
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
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RREQ";

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new DsrRouteRequestMessage()
            {
                RequestId = this.RequestId,
                Nodes = new List<string>(this.Nodes)
            };

            return CopyTo(clone);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{base.ToString()}\n| Nodes:{string.Join(",", Nodes)}\n"
                   + $"| Request:{RequestId}\n+[/{this.GetType().Name}]";
        }
    }
}
