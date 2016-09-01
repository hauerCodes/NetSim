﻿using System;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Routing.DSDV
{
    /// <summary>
    /// 
    /// </summary>
    public class DsdvSequence : NetSimSequence, IEquatable<DsdvSequence>, IComparable<DsdvSequence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DsdvSequence"/> class.
        /// </summary>
        public DsdvSequence() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsdvSequence"/> class.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public DsdvSequence(string sequenceId, int sequenceNr) : base(sequenceId, sequenceNr)
        { }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(DsdvSequence other)
        {
            return base.Equals(other);
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">Can't compare this sequences.</exception>
        public int CompareTo(DsdvSequence other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new DsdvSequence(this.SequenceId, this.SequenceNr);
        }

    }
}