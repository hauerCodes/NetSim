using System;
using System.Linq;

namespace NetSim.Lib.Simulator
{
    /// <summary>
    /// 
    /// </summary>
    public enum NetSimMessageTransmissionStep
    {
        /// <summary>
        /// The sending transmission step - from EndPoint A to Mid
        /// </summary>
        Sending,

        /// <summary>
        /// The receiving transmission step - from mid to endpoint B
        /// </summary>
        Receiving,

        /// <summary>
        /// The done step
        /// </summary>
        Done
    }
}