using System;
using System.Linq;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Routing.OLSR
{
    /// <summary>
    /// 
    /// </summary>
    public class OlsrSequence : NetSimSequence, IEquatable<OlsrSequence>, IComparable<OlsrSequence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrSequence"/> class.
        /// </summary>
        public OlsrSequence() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrSequence"/> class.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public OlsrSequence(string sequenceId, int sequenceNr) : base(sequenceId, sequenceNr)
        { }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(OlsrSequence other)
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
        public int CompareTo(OlsrSequence other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new OlsrSequence(this.SequenceId, this.SequenceNr);
        }

    }
}