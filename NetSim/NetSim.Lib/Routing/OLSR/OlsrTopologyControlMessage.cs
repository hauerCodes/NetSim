using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrTopologyControlMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrTopologyControlMessage"/> class.
        /// </summary>
        public OlsrTopologyControlMessage()
        {
            MultiPointRelaySelectorSet = new List<string>();
        }

        /// <summary>
        /// Gets or sets the multi point relays.
        /// </summary>
        /// <value>
        /// The multi point relays.
        /// </value>
        public List<string> MultiPointRelaySelectorSet { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public OlsrSequence SequenceNr { get; set; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public override string ShortName => "TC";

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new OlsrTopologyControlMessage()
            {
                SequenceNr = (OlsrSequence)this.SequenceNr?.Clone(),
                MultiPointRelaySelectorSet = new List<string>(this.MultiPointRelaySelectorSet)
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
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(base.ToString());

            if (SequenceNr != null)
            {
                builder.AppendFormat("| SequenceNr: {0}\n", SequenceNr);
            }

            if (MultiPointRelaySelectorSet != null && MultiPointRelaySelectorSet.Count > 0)
            {
                builder.AppendFormat("| MPR-SelectorSet: {0}\n", string.Join(",", MultiPointRelaySelectorSet));
            }

            builder.AppendLine($"+[/{this.GetType().Name}]");


            return builder.ToString();
        }
    }
}
