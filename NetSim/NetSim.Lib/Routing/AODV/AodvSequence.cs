// -----------------------------------------------------------------------
// <copyright file="AodvSequence.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - AodvSequence.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Routing.AODV
{
    using System;
    using System.Linq;
    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The Aodv Sequence nr class.
    /// </summary>
    public class AodvSequence : NetSimSequence, IEquatable<AodvSequence>, IComparable<AodvSequence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AodvSequence"/> class.
        /// </summary>
        public AodvSequence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AodvSequence"/> class.
        /// </summary>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="sequenceNr">The sequence nr.</param>
        public AodvSequence(string sequenceId, int sequenceNr)
            : base(sequenceId, sequenceNr)
        {
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>The cloned sequence object.</returns>
        public override object Clone()
        {
            return new AodvSequence(this.SequenceId, this.SequenceNr);
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero This object is less than the
        /// <paramref name="other" /> parameter.Zero
        /// This object is equal to <paramref name="other" />.
        /// Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">If other is null.</exception>
        /// <exception cref="System.InvalidOperationException">Can't compare this sequences if sequence ids are unequal.</exception>
        public int CompareTo(AodvSequence other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares the specified other with this instance.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(AodvSequence other)
        {
            return base.Equals(other);
        }
    }
}