using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using NetSim.Lib;
using NetSim.Lib.Simulator;
using NetSim.Lib.Visualization;

namespace NetSim.Lib.Simulator
{
    public class NetSimConnection : NetSimItem
    {
        /// <summary>
        /// Occurs when clientStateUpdate.
        /// </summary>
        private event Action ConnectionStateUpdate;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimConnection"/> class.
        /// </summary>
        public NetSimConnection()
        {
            this.PendingMessages = new Queue<NetSimMessage>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the pending messages.
        /// </summary>
        /// <value>
        /// The pending messages.
        /// </value>
        public Queue<NetSimMessage> PendingMessages { get; set; }

        /// <summary>
        /// Gets or sets the end point a.
        /// </summary>
        /// <value>
        /// The end point a.
        /// </value>
        public NetSimClient EndPointA { get; set; }

        /// <summary>
        /// Gets or sets the end point b.
        /// </summary>
        /// <value>
        /// The end point b.
        /// </value>
        public NetSimClient EndPointB { get; set; }

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public int Metric { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is offline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        public bool IsOffline { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether this instance is transmitting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is transmitting; otherwise, <c>false</c>.
        /// </value>
        public bool IsTransmitting => PendingMessages.Count > 0;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when client state is updated due routing or other events.
        /// </summary>
        public event Action StateUpdated
        {
            add
            {
                ConnectionStateUpdate += value;
            }
            remove
            {
                ConnectionStateUpdate -= value;
            }
        }

        #endregion

        /// <summary>
        /// Starts the transport message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void StartTransportMessage(NetSimMessage message)
        {
            if (EndPointA == null || EndPointB == null || IsOffline) return;

            if (EndPointA.Id.Equals(message.Receiver) || EndPointB.Id.Equals(message.Receiver))
            {
                PendingMessages.Enqueue(message);
                OnStateUpdated();
            }
        }

        /// <summary>
        /// Ends the transport messages.
        /// </summary>
        public void EndTransportMessages()
        {
            while(PendingMessages.Count > 0 )
            {
                var message = PendingMessages.Dequeue();

                if (EndPointA != null && message.Receiver.Equals(EndPointA.Id))
                {
                    EndPointA.ReceiveMessage(message);
                    OnStateUpdated();
                }

                if (EndPointB != null && message.Receiver.Equals(EndPointB.Id))
                {
                    EndPointB.ReceiveMessage(message);
                    OnStateUpdated();
                }
            }
        }

        /// <summary>
        /// Called when the state is updated.
        /// </summary>
        protected void OnStateUpdated()
        {
            ConnectionStateUpdate?.Invoke();
        }

    }
}