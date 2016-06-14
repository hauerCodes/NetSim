using System;
using System.Collections.Generic;
using System.Linq;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Visualization
{
    public interface IDrawableNetSimSimulator
    {
        event Action Updated;
        List<NetSimClient> Clients { get; set; }
        List<NetSimConnection> Connections { get; set; }
    }
}