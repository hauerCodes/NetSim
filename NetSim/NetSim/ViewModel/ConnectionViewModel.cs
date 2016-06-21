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
        public ConnectionViewModel(NetSimConnection connection)
        {
            this.Connection = connection;
        }

        public NetSimConnection Connection { get; }

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
