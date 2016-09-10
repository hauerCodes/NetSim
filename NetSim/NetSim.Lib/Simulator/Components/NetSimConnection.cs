// -----------------------------------------------------------------------
// <copyright file="NetSimConnection.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimConnection.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Simulator.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The connection class that represents a edge in the network.
    /// </summary>
    /// <seealso cref="NetSim.Lib.Simulator.Components.NetSimItem" />
    public class NetSimConnection : NetSimItem
    {
        /// <summary>
        /// The is offline
        /// </summary>
        private bool isOffline;

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
        /// Occurs when client state is updated due routing or other events.
        /// </summary>
        public event Action StateUpdated
        {
            add
            {
                this.ConnectionStateUpdate += value;
            }

            remove
            {
                this.ConnectionStateUpdate -= value;
            }
        }

        /// <summary>
        /// Occurs when clientStateUpdate.
        /// </summary>
        private event Action ConnectionStateUpdate;

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
        /// Gets or sets a value indicating whether this instance is offline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline; otherwise, <c>false</c>.
        /// </value>
        public bool IsOffline
        {
            get
            {
                return this.isOffline;
            }

            set
            {
                this.isOffline = value;
                this.OnStateUpdated();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is transmitting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is transmitting; otherwise, <c>false</c>.
        /// </value>
        public bool IsTransmitting => this.PendingMessages.Count > 0;

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public int Metric { get; set; } = 1;

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
        /// Ends the transport messages.
        /// </summary>
        public void EndTransportMessages()
        {
            // remove all transmitted message that are done
            var doneList =
                this.TransmittedMessages.Where(m => m.TransmissionStep == NetSimMessageTransmissionStep.Done).ToList();
            doneList.ForEach(m => this.TransmittedMessages.Remove(m));

            // mark all already transmitted messages that are in receicing mode as done
            this.TransmittedMessages.Where(m => m.TransmissionStep == NetSimMessageTransmissionStep.Receiving)
                .ToList()
                .ForEach(m => m.TransmissionStep = NetSimMessageTransmissionStep.Done);

            // send messages to receiver endpoint
            while (this.PendingMessages.Count > 0)
            {
                ConnectionFrameMessage frame = this.PendingMessages.Dequeue();

                if (frame == null)
                {
                    continue;
                }

                // mark each message as transmission receiving (triggers the receiving animation)
                if (frame.TransmissionStep == NetSimMessageTransmissionStep.Sending)
                {
                    frame.TransmissionStep = NetSimMessageTransmissionStep.Receiving;
                }

                if (this.EndPointA != null && frame.Receiver.Equals(this.EndPointA.Id))
                {
                    this.EndPointA.ReceiveMessage(this.UnWrapMessage(frame));
                }

                if (this.EndPointB != null && frame.Receiver.Equals(this.EndPointB.Id))
                {
                    this.EndPointB.ReceiveMessage(this.UnWrapMessage(frame));
                }

                // add received message to transmitted message for visualization
                this.TransmittedMessages.Add(frame);
            }

            this.OnStateUpdated();
        }

        /// <summary>
        /// Starts the transport message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="senderEndPoint">The sender end point.</param>
        /// <param name="receiverEndPoint">The receiver end point.</param>
        public void StartTransportMessage(NetSimMessage message, string senderEndPoint, string receiverEndPoint)
        {
            // compared with ethernet frame - mac address
            // only necessary for the connection class knows which end to transmit message
            var frameMessage = this.WrapMessage(message, senderEndPoint, receiverEndPoint);

            // start message transport
            if (this.EndPointA == null || this.EndPointB == null || this.IsOffline)
            {
                return;
            }

            if (this.EndPointA.Id.Equals(frameMessage.Receiver) || this.EndPointB.Id.Equals(frameMessage.Receiver))
            {
                this.PendingMessages.Enqueue(frameMessage);
                this.OnStateUpdated();
            }
        }

        /// <summary>
        /// Called when the state is updated.
        /// </summary>
        protected void OnStateUpdated()
        {
            this.ConnectionStateUpdate?.Invoke();
        }

        /// <summary>
        /// Unwraps the message from the connection frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The unwrapped message.</returns>
        private NetSimMessage UnWrapMessage(ConnectionFrameMessage frame)
        {
            return frame.InnerMessage;
        }

        /// <summary>
        /// Wraps the message in the connection frame.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="senderEndPoint">The sender end point.</param>
        /// <param name="receiverEndPoint">The receiver end point.</param>
        /// <returns>The frame wrapped message.</returns>
        private ConnectionFrameMessage WrapMessage(
            NetSimMessage message,
            string senderEndPoint,
            string receiverEndPoint)
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
    }
}