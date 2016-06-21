using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.DSDV
{
    public class DsdvUpdateMessage : NetSimMessage
    {
        public DsdvTable UpdateTable { get; set; }

        public override object Clone()
        {
            return new DsdvUpdateMessage()
            {
                Sender = this.Sender,
                Receiver = this.Receiver,
                UpdateTable = (DsdvTable)UpdateTable.Clone()
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}\n---\n{UpdateTable}";
        }

    }
}
