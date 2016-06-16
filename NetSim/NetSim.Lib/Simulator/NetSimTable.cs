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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimTable"/> class.
        /// </summary>
        protected NetSimTable()
        {
            this.Entries = new List<NetSimTableEntry>();
        }

        #endregion

        #region Events

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<NetSimTableEntry> Entries { get; set; }

        #endregion

        /// <summary>
        /// Should be called when table gets updated.
        /// </summary>
        protected virtual void OnTableUpdate()
        {
            TableUpdate?.Invoke();
        }

        public abstract NetSimTableEntry GetRouteFor(string destinationId);

        public abstract object Clone();
    }
}