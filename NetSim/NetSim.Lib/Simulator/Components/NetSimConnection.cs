using System;
using System.Collections.Generic;
using System.Linq;

using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Simulator.Components
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
            this.PendingMessages = new Queue<ConnectionFrameMessage>();
            this.TransmittedMessages = new List<ConnectionFrameMessage>();
        }

        /// <summary>
        /// Gets or sets the pending messages.
        /// </summary>
        /// <value>
        /// The pending messages.
        /// </value>
        public Queue<ConnectionFrameMessage> PendingMessages { get; set; }

        /// <summary>
        /// Gets or sets the transmitted messages.
        /// </summary>
        /// <value>
        /// The transmitted messages.
        /// </value>
        public List<ConnectionFrameMessage> TransmittedMessages { get; set; }

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
        /// <param name="senderEndPoint">The sender end point.</param>
        /// <param name="receiverEndPoint">The receiver end point.</param>
        public void StartTransportMessage(NetSimMessage message, string senderEndPoint, string receiverEndPoint)
        {
            var frameMessage = WrapMessage(message, senderEndPoint, receiverEndPoint);

            //TODO
            // hack set the "next" receiver 
            // compared with ethernet frame - mac address
            // only necessary for the connection class knows which end to transmit message
            //message.NextReceiver = receiverEndPoint;
            //message.TransmissionStep = NetSimMessageTransmissionStep.Sending;

            // start message transport
            if (EndPointA == null || EndPointB == null || IsOffline) return;

            if (EndPointA.Id.Equals(frameMessage.Receiver) || EndPointB.Id.Equals(frameMessage.Receiver))
            {
                PendingMessages.Enqueue(frameMessage);
                OnStateUpdated();
            }
        }

        /// <summary>
        /// Wraps the message in the connection frame.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="senderEndPoint">The sender end point.</param>
        /// <param name="receiverEndPoint">The receiver end point.</param>
        /// <returns></returns>
        private ConnectionFrameMessage WrapMessage(NetSimMessage message, string senderEndPoint, string receiverEndPoint)
        {
            ConnectionFrameMessage frame = new ConnectionFrameMessage
            {
                InnerMessage = message,
                Sender = senderEndPoint,
                Receiver = receiverEndPoint,
                TransmissionStep = NetSimMessageTransmissionStep.Sending
            };

            return frame;
        }

        /// <summary>
        /// Unwraps the message from the connection frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        private NetSimMessage UnWrapMessage(ConnectionFrameMessage frame)
        {
            return frame.InnerMessage;
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

            // send messages to receiver endpoint
            while (PendingMessages.Count > 0)
            {
                ConnectionFrameMessage frame = PendingMessages.Dequeue() as ConnectionFrameMessage;

                if (frame == null) continue;

                // mark each message as transmission receiving (triggers the receiving animation)
                if (frame.TransmissionStep == NetSimMessageTransmissionStep.Sending)
                {
                    frame.TransmissionStep = NetSimMessageTransmissionStep.Receiving;
                }

                if (EndPointA != null && frame.Receiver.Equals(EndPointA.Id))
                {
                    EndPointA.ReceiveMessage(UnWrapMessage(frame));
                }

                if (EndPointB != null && frame.Receiver.Equals(EndPointB.Id))
                {
                    EndPointB.ReceiveMessage(UnWrapMessage(frame));
                }

                // add received message to transmitted message for visualization
                TransmittedMessages.Add(frame);
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