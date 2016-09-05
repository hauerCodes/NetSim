using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvRouteReplyMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the hop count.
        /// </summary>
        /// <value>
        /// The hop count.
        /// </value>
        public int HopCount { get; set; }

        /// <summary>
        /// Gets or sets the last hop.
        /// Note: This Info has to be saved to determine from 
        /// which interface or hop this message has been sent 
        /// </summary>
        /// <value>
        /// The last hop.
        /// </value>
        public string LastHop { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public AodvSequence ReceiverSequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "RREP";

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return CopyTo(new AodvRouteReplyMessage()
            {
                HopCount = this.HopCount,
                ReceiverSequenceNr = (AodvSequence)this.ReceiverSequenceNr.Clone()
            });
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{base.ToString()}\n| HopCount:{HopCount}\n+[/{this.GetType().Name}]";
        }
    }
}
