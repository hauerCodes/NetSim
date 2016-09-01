using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrHelloMessage : NetSimMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrHelloMessage"/> class.
        /// </summary>
        public OlsrHelloMessage()
        {
            this.Neighbors = new List<string>();
        }

        /// <summary>
        /// Gets or sets the neighbors.
        /// </summary>
        /// <value>
        /// The neighbors.
        /// </value>
        public List<string> Neighbors { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = new OlsrHelloMessage() { Neighbors = new List<string>(Neighbors) };

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
            builder.AppendLine("--- Content ---");

            builder.AppendLine(string.Join(" ", Neighbors));

            return builder.ToString();
        }
    }
}
