using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.AODV
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="NetSimMessage" />
    public class AodvHelloMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the not reachable node sequence nr.
        /// </summary>
        /// <value>
        /// The not reachable node sequence nr.
        /// </value>
        public AodvSequence SenderSequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "Hello"; 

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return CopyTo(new AodvHelloMessage()
            {
                SenderSequenceNr = (AodvSequence)this.SenderSequenceNr?.Clone()
            });
        }
    }
}
