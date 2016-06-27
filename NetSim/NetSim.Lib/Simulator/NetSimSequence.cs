using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Simulator
{
    public class NetSimSequence : IEquatable<NetSimSequence>, IComparable<NetSimSequence>, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimSequence"/> class.
        /// </summary>
        public NetSimSequence() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimSequence"/> class.
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
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(NetSimSequence other)
        {
            if (other == null) return false;

            return (this.SequenceId.Equals(other.SequenceId) && this.SequenceNr == other.SequenceNr);
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.InvalidOperationException">Can't compare this sequences.</exception>
        public int CompareTo(NetSimSequence other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

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
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new NetSimSequence() { SequenceId = this.SequenceId, SequenceNr = this.SequenceNr };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{SequenceId}-{SequenceNr:000}";
        }
    }
}
