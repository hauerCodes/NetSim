using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetSim.Lib.Simulator;

namespace NetSim.Lib.Routing.OLSR
{
    public class OlsrRoutingProtocol : NetSimRoutingProtocol
    {

        #region Constructor 

        /// <summary>
        /// Initializes a new instance of the <see cref="OlsrRoutingProtocol"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public OlsrRoutingProtocol(NetSimClient client) : base(client) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the one hop neighbor table.
        /// </summary>
        /// <value>
        /// The one hop neighbor table.
        /// </value>
        public OlsrNeighborTable OneHopNeighborTable { get; set; }

        /// <summary>
        /// Gets or sets the two hop neighbor table.
        /// </summary>
        /// <value>
        /// The two hop neighbor table.
        /// </value>
        public OlsrNeighborTable TwoHopNeighborTable { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public OlsrState State { get; set; }

        #endregion

        public override void Initialize()
        {
            // call base initialization (stepcounter and data)
            base.Initialize();

            //intialize local neighbor tables
            this.OneHopNeighborTable = new OlsrNeighborTable();
            this.TwoHopNeighborTable = new OlsrNeighborTable();

            //set protocol state 
            this.State = OlsrState.HelloOneHop;
        }

        public override void PerformRoutingStep()
        {
            switch (State)
            {
                case OlsrState.Hello:
                    // send hello message to all direct links
                    BroadcastHelloMessages();

                    State = OlsrState.WaitForHello;
                    break;

                case OlsrState.WaitForHello:
                    // wait for incoming hello messages 
                    ReceiveHelloMessages();

                    // wait unitl every connected links has sent his hello
                    if(this.OneHopNeighborTable.Entries.Count >= this.Client.Connections.Count(x => !x.Value.IsOffline))
                    {
                        this.State = OlsrState.Hello;
                    }
                    break;

                case OlsrState.TopologyControl:
                    break;
            }

            stepCounter++;
        }

        private void BroadcastHelloMessages()
        {
            throw new NotImplementedException();
        }

        private void ReceiveHelloMessages()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether [is one hop neighbor] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool IsOneHopNeighbor(string id)
        {
            return OneHopNeighborTable.GetEntryFor(id) != null;
        }

        /// <summary>
        /// Determines whether [is two hop neighbor] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool IsTwoHopNeighbor(string id)
        {
            return TwoHopNeighborTable.GetEntryFor(id) != null;
        }
    }

    public enum OlsrState
    {
        Hello,
        WaitForHello,
        TopologyControl
    }
}
