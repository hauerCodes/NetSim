using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.AODV
{
    public class AodvRoutingProtocol : NetSimRoutingProtocol
    {
        public AodvRoutingProtocol(NetSimClient client)
            : base(client)
        {
        }

        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

        }

        public override void PerformRoutingStep()
        {
            throw new NotImplementedException();
        }
    }
}
