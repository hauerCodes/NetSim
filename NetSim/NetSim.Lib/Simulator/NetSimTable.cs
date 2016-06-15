using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSim.Lib.Simulator
{
    public abstract class NetSimTable
    {
        public List<NetSimTableEntry> Entries
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
            }
        }
    }
}