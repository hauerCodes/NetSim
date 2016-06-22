using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

using NetSim.Lib.Simulator;

namespace NetSim.ViewModel
{
    public class ConnectionViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionViewModel"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ConnectionViewModel(NetSimConnection connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public NetSimConnection Connection { get; }

        /// <summary>
        /// Gets the current messages.
        /// </summary>
        /// <value>
        /// The current messages.
        /// </value>
        public string CurrentMessages
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach(var message in this.Connection.PendingMessages)
                {
                    builder.AppendLine(message.ToString());
                }

                return builder.ToString();
            }
        }
    }

}
