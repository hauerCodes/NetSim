using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Storage
{
    [Serializable]
    public class StorageClient
    {
        public string Id { get; set; }

        public int Top { get; set; }

        public int Left { get; set; }

        public bool IsOffline { get; set; }
    }
}
