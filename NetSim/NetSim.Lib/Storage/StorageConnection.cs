using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Storage
{
    [Serializable]
    public class StorageConnection
    {
        public string Id { get; set; }

        public string EndpointA { get; set; }

        public string EndpointB { get; set; }

        public bool IsOffline { get; set; }

        public int Metric { get; set; }
    }
}
