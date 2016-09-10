// -----------------------------------------------------------------------
// <copyright file="NetSimLocation.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimLocation.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Visualization
{
    /// <summary>
    /// Note not a struct here - because:
    /// To enable edit on the same object reference for placement.
    /// </summary>
    public class NetSimLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimLocation"/> class.
        /// </summary>
        public NetSimLocation()
        {
            this.Left = 0;
            this.Top = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimLocation"/> class.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        public NetSimLocation(int left, int top)
        {
            this.Left = left;
            this.Top = top;
        }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        /// <value>
        /// The left.
        /// </value>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>
        /// The top.
        /// </value>
        public int Top { get; set; }
    }
}