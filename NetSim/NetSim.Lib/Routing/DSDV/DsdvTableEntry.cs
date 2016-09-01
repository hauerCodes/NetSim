using System;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.DSDV
{
    public class DsdvTableEntry : NetSimTableEntry
    {
        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public DsdvSequence SequenceNr { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new DsdvTableEntry()
            {
                Destination = this.Destination,
                Metric = this.Metric,
                NextHop = this.NextHop, 
                SequenceNr = (DsdvSequence)SequenceNr.Clone()
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString() + $" {SequenceNr, 5}";
        }
    }
}