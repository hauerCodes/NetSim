using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSim.Lib.Simulator
{
    public class NetSimConnection : NetSimItem
    {
        /// <summary>
        /// The is offline
        /// </summary>
        private bool isOffline;

        /// <summary>
        /// Occurs when clientStateUpdate.
        /// </summary>
        private event Action ConnectionStateUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimConnection"/> class.
        /// </summary>
        public NetSimConnection()
        {
            this.IsOffline = false;
            this.PendingMessages = new Queue<NetSimMessage>();
            this.TransmittedMessages = new List<NetSimMessage>();
        }

        /// <summary>
        /// Gets or sets the pending messages.
        /// </summary>
        /// <value>
        /// The pending messages.
        /// </value>
        public Queue<NetSimMessage> PendingMessages { get; set; }

        /// <summary>
        /// Gets or sets the transmitted messages.
        /// </summary>
        /// <value>
        /// The transmitted messages.
        /// </value>
        public List<NetSimMessage> TransmittedMessages { get; set; }

        /// <summary>
        /// Gets or sets the end point a.
        /// </summary>
        /// <value>
        /// The end point a.
        /// </value>
        public INetSimConnectionEndpoint EndPointA { get; set; }

        /// <summary>
        /// Gets or sets the end point b.
        /// </summary>
        /// <value>
        /// The end point b.
        /// </value>
        public INetSimConnectionEndpoint EndPointB { get; set; }

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
        public bool IsOffline
        {
            get
            {
                return isOffline;
            }
            set
            {
                isOffline = value;
                OnStateUpdated();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is transmitting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is transmitting; otherwise, <c>false</c>.
        /// </value>
        public bool IsTransmitting => PendingMessages.Count > 0;

        /// <summary>
        /// Gets a value indicating whether this instance is cleanup necessary.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is cleanup necessary; otherwise, <c>false</c>.
        /// </value>
        public bool IsCleanupNecessary => TransmittedMessages.Any(tm => tm.TransmissionStep == NetSimMessageTransmissionStep.Receiving);

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

        /// <summary>
        /// Starts the transport message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="receiverEndPoint">The receiver end point.</param>
        public void StartTransportMessage(NetSimMessage message, string receiverEndPoint)
        {
            // hack set the "next" receiver 
            // compared with ethernet frame - mac address
            // only necessary for the connection class knows which end to transmit message
            message.NextReceiver = receiverEndPoint;

            message.TransmissionStep = NetSimMessageTransmissionStep.Sending;

            // start message transport
            if (EndPointA == null || EndPointB == null || IsOffline) return;

            if (EndPointA.Id.Equals(message.NextReceiver) || EndPointB.Id.Equals(message.NextReceiver))
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
            // remove all transmitted message that are done
            var doneList =
                TransmittedMessages.Where(m => m.TransmissionStep == NetSimMessageTransmissionStep.Done).ToList();
            doneList.ForEach(m => TransmittedMessages.Remove(m));

            // mark all already transmitted messages that are in receicing mode as done
            TransmittedMessages
                .Where(m => m.TransmissionStep == NetSimMessageTransmissionStep.Receiving).ToList()
                .ForEach(m => m.TransmissionStep = NetSimMessageTransmissionStep.Done);

            OnStateUpdated();

            // send messages to receiver endpoint
            while (PendingMessages.Count > 0)
            {
                var message = PendingMessages.Dequeue();

                // mark each message as transmission receiving (triggers the receiving animation)
                if (message.TransmissionStep == NetSimMessageTransmissionStep.Sending)
                {
                    message.TransmissionStep = NetSimMessageTransmissionStep.Receiving;
                }

                if (EndPointA != null && message.NextReceiver.Equals(EndPointA.Id))
                {
                    EndPointA.ReceiveMessage(message);
                    // OnStateUpdated();
                }

                if (EndPointB != null && message.NextReceiver.Equals(EndPointB.Id))
                {
                    EndPointB.ReceiveMessage(message);
                    // OnStateUpdated();
                }

                // add received message to transmitted message for visualization
                TransmittedMessages.Add(message);
            }

            OnStateUpdated();
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