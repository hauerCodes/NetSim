using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

using NetSim.Lib.Simulator;

namespace NetSim.ViewModels
{
    public class ConnectionViewModel : ViewModelBase
    {
        private NetSimConnection connection;

        public ConnectionViewModel(NetSimConnection connection)
        {
            this.connection = connection;
        }

        public NetSimConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }


    }

}
