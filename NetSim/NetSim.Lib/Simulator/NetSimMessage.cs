using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public abstract class NetSimMessage : NetSimItem, ICloneable
    {
        public string Sender { get; set; }

        public string Receiver { get; set; }

        public abstract object Clone();

        public override string ToString()
        {
            return $"#[{this.GetType().Name}] {Sender} - {Receiver}";
        }
    }
}