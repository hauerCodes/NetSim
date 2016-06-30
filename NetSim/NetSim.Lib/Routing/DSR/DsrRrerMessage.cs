using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSR
{
    public class DsrRrerMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the not reachable node.
        /// </summary>
        /// <value>
        /// The not reachable node.
        /// </value>
        public string NotReachableNode { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new DsrRrerMessage()
            {
                Sender = this.Sender,
                Receiver = this.Receiver,
                NotReachableNode = this.NotReachableNode
            };
        }
    }
}
