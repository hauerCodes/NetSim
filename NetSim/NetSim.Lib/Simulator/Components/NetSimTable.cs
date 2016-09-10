// -----------------------------------------------------------------------
// <copyright file="NetSimTable.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimTable.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The base class for all routing table implementations.
    /// </summary>
    /// <seealso cref="System.ICloneable" />
    public abstract class NetSimTable : ICloneable
    {
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
                this.TableUpdate += value;
            }

            remove
            {
                this.TableUpdate -= value;
            }
        }

        /// <summary>
        /// Should be invoked when table gets updated.
        /// </summary>
        private event Action TableUpdate;

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<NetSimTableEntry> Entries { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public abstract object Clone();

        /// <summary>
        /// Gets the route for.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns>The found route for the destination or null.</returns>
        public abstract NetSimTableEntry GetRouteFor(string destinationId);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var entry in this.Entries)
            {
                builder.AppendLine(entry.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Should be called when table gets updated.
        /// </summary>
        protected virtual void OnTableUpdate()
        {
            this.TableUpdate?.Invoke();
        }
    }
}