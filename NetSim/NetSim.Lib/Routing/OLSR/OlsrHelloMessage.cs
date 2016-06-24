using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrHelloMessage : NetSimMessage
    {

        public OlsrHelloMessage()
        {
               
        }
        public List<string> Neighbors { get; set; }

        public override object Clone()
        {
            return new OlsrHelloMessage();
        }
    }
}
