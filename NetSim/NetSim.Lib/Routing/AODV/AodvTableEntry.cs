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
    /// <seealso cref="NetSimTableEntry" />
    public class AodvTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public AodvSequence SequenceNr { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new AodvTableEntry()
            {
                Destination = this.Destination,
                Metric = this.Metric,
                NextHop = this.NextHop,
                SequenceNr = (AodvSequence)this.SequenceNr.Clone()
            };
        }
    }
}
