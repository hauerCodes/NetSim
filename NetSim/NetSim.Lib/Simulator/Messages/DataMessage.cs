using System;
using System.Linq;

using NetSim.Lib.Simulator.Components;

namespace NetSim.Lib.Simulator.Messages
{
    public class DataMessage : NetSimMessage
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public string Data { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return CopyTo(new DataMessage() { Data = this.Data });
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{base.ToString()}\n| {this.Data}\n+[/{this.GetType().Name}]";
        }
    }
}
