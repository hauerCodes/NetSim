using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Storage
{
    public class StorageNetwork
    {
        public StorageClient[] Clients { get; set; }

        public StorageConnection[] Connections { get; set; }

    }
}
