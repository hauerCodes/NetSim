using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSim.Lib.Simulator
{
    public abstract class NetSimTable : ICloneable
    {
        /// <summary>
        /// Should be invoked when table gets updated.
        /// </summary>
        private event Action TableUpdate;

        /// <summary>
        /// The not reachable
        /// </summary>
        protected const int NotReachable = -1;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimTable"/> class.
        /// </summary>
        protected NetSimTable()
        {
            this.Entries = new List<NetSimTableEntry>();
        }

        

        
        /// <summary>
        /// Occurs when table gets updated.
        /// </summary>
        public event Action TableUpdated
        {
            add
            {
                TableUpdate += value;
            }
            remove
            {
                TableUpdate -= value;
            }

        }

        

        
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<NetSimTableEntry> Entries { get; set; }

        

        /// <summary>
        /// Should be called when table gets updated.
        /// </summary>
        protected virtual void OnTableUpdate()
        {
            TableUpdate?.Invoke();
        }

        /// <summary>
        /// Gets the route for.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns></returns>
        public abstract NetSimTableEntry GetRouteFor(string destinationId);

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach(var entry in Entries)
            {
                builder.AppendLine(entry.ToString());
            }
            return builder.ToString();
        }
    }
}