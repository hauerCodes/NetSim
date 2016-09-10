// -----------------------------------------------------------------------
// <copyright file="NetSimMessageTransmissionStep.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimMessageTransmissionStep.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Messages
{
    using System;
    using System.Linq;

    /// <summary>
    /// The message transmission step enumeration.
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