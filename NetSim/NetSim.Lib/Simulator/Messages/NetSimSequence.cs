// -----------------------------------------------------------------------
// <copyright file="NetSimSequence.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimSequence.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Messages
{
    using System;

    /// <summary>
    /// The Sequence base class.
    /// </summary>
    /// <seealso cref="System.IEquatable{NetSimSequence}" />
    /// <seealso cref="System.IComparable{NetSimSequence}" />
    /// <seealso cref="System.ICloneable" />
    public class NetSimSequence : IEquatable<NetSimSequence>, IComparable<NetSimSequence>, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimSequence" /> class.
        /// </summary>
        public NetSimSequence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimSequence" /> class.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public NetSimSequence(string sequenceId, int sequenceNr)
        {
            this.SequenceId = sequenceId;
            this.SequenceNr = sequenceNr;
        }

        /// <summary>
        /// Gets or sets the sequence identifier.
        /// </summary>
        /// <value>
        /// The sequence identifier.
        /// </value>
        public string SequenceId { get; set; }

        /// <summary>
        /// Gets or sets the sequence nr.
        /// </summary>
        /// <value>
        /// The sequence nr.
        /// </value>
        public int SequenceNr { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone()
        {
            return new NetSimSequence(this.SequenceId, this.SequenceNr);
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. 
        /// The return value has the following meanings: Value Meaning Less than zero 
        /// This object is less than the <paramref name="other" /> parameter.
        /// Zero This object is equal to <paramref name="other" />. 
        /// Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">Can't compare this sequences.</exception>
        /// <exception cref="System.ArgumentNullException">If argument other is null.</exception>
        /// <exception cref="System.InvalidOperationException">Can't compare this sequences.</exception>
        public int CompareTo(NetSimSequence other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (!other.SequenceId.Equals(this.SequenceId))
            {
                throw new InvalidOperationException("Can't compare this sequences.");
            }

            if (this.SequenceNr > other.SequenceNr)
            {
                return 1;
            }

            if (this.SequenceNr < other.SequenceNr)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Compared the specified other instance to this instance.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(NetSimSequence other)
        {
            if (other == null)
            {
                return false;
            }

            return this.SequenceId.Equals(other.SequenceId) && this.SequenceNr == other.SequenceNr;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.SequenceId}-{this.SequenceNr:000}";
        }
    }
}