using System;
using System.Linq;

namespace NetSim.Lib.Visualization
{

    /// <summary>
    ///  // Note not a struct here - because:
    /// </summary>
    public class NetSimLocation
    {
        public NetSimLocation()
        {
            this.Left = 0;
            this.Top = 0;
        }

        public NetSimLocation(int left, int top)
        {
            this.Left = left;
            this.Top = top;
        }

        public int Left { get; set; }

        public int Top { get; set; }
    }
}
