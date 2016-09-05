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
        /// Initializes a new instance of the <see cref="AodvTableEntry"/> class.
        /// </summary>
        public AodvTableEntry()
        {
            this.ActiveNeighbours = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public AodvSequence SequenceNr { get; set; }

        /// <summary>
        /// Gets or sets the active neighbors dictonary. 
        /// The key identifies the neighbor. 
        /// The value part is a time counter.
        /// </summary>
        /// <value>
        /// The active neighbors.
        /// </value>
        public Dictionary<string, int> ActiveNeighbours { get; set; }

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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString() + $" {SequenceNr,5} {String.Join(",", ActiveNeighbours.Keys)}";
        }
    }
}
