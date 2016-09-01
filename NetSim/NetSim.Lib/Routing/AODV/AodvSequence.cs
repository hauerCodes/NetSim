using System;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Routing.AODV
{
    /// <summary>
    /// 
    /// </summary>
    public class AodvSequence : NetSimSequence, IEquatable<AodvSequence>, IComparable<AodvSequence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AodvSequence"/> class.
        /// </summary>
        public AodvSequence() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AodvSequence"/> class.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public AodvSequence(string sequenceId, int sequenceNr) : base(sequenceId, sequenceNr)
        { }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(AodvSequence other)
        {
            return base.Equals(other);
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has the following meanings: Value Meaning Less than zero This object is less than the
        ///  <paramref name="other" /> parameter.Zero 
        /// This object is equal to <paramref name="other" />. 
        /// Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">Can't compare this sequences.</exception>
        public int CompareTo(AodvSequence other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new AodvSequence(this.SequenceId, this.SequenceNr);
        }

    }
}