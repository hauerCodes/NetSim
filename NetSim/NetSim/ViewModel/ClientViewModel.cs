using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

using NetSim.Lib.Simulator;

namespace NetSim.ViewModel
{
    public class ClientViewModel : ViewModelBase
    {
        private readonly NetSimClient client;

        public ClientViewModel(NetSimClient client)
        {
            this.client = client;
        }

    }
}
