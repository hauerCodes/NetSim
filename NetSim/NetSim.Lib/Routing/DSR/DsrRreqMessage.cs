using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.NetSimMessage" />
    public class DsrRreqMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsrRreqMessage" /> class.
        /// </summary>
        public DsrRreqMessage()
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
            return new DsrRreqMessage()
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
