// -----------------------------------------------------------------------
// <copyright file="OlsrSequence.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - OlsrSequence.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.OLSR
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The olsr sequence implementation.
    /// </summary>
    public class OlsrSequence : NetSimSequence, IEquatable<OlsrSequence>, IComparable<OlsrSequence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrSequence"/> class.
        /// </summary>
        public OlsrSequence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrSequence"/> class.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public OlsrSequence(string sequenceId, int sequenceNr)
            : base(sequenceId, sequenceNr)
        {
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return new OlsrSequence(this.SequenceId, this.SequenceNr);
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
        /// <exception cref="System.ArgumentNullException">If argument is null.</exception>
        /// <exception cref="System.InvalidOperationException">Can't compare this sequences.</exception>
        public int CompareTo(OlsrSequence other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(OlsrSequence other)
        {
            return base.Equals(other);
        }
    }
}