// -----------------------------------------------------------------------
// <copyright file="NetSimVisualizer.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - NetSimVisualizer.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Visualization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;

    using NetSim.Lib.Controls;
    using NetSim.Lib.Routing.AODV;
    using NetSim.Lib.Routing.DSDV;
    using NetSim.Lib.Routing.DSR;
    using NetSim.Lib.Routing.OLSR;
    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The visualizer class contains the logic for displaying the current simulation step
    /// on the given draw canvas.
    /// </summary>
    public class NetSimVisualizer
    {
        /// <summary>
        /// The draw canvas
        /// </summary>
        private readonly Canvas drawCanvas;

        /// <summary>
        /// The message states
        /// </summary>
        private readonly Dictionary<string, NetSimMessageTransmissionStep> messageStates;

        /// <summary>
        /// The simulator
        /// </summary>
        private readonly IDrawableNetSimSimulator simulator;

        /// <summary>
        /// The current selected item
        /// </summary>
        private NetSimClient currentSelectedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimVisualizer"/> class.
        /// </summary>
        /// <param name="simulator">The simulator.</param>
        /// <param name="drawCanvas">The draw canvas.</param>
        public NetSimVisualizer(IDrawableNetSimSimulator simulator, Canvas drawCanvas)
        {
            this.simulator = simulator;
            this.drawCanvas = drawCanvas;
            this.messageStates = new Dictionary<string, NetSimMessageTransmissionStep>();

            simulator.SimulatorUpdated += this.SimulatorUpdated;
        }

        /// <summary>
        /// Gets or sets the current selected item.
        /// </summary>
        /// <value>
        /// The current selected item.
        /// </value>
        public NetSimClient CurrentSelectedItem
        {
            get
            {
                return this.currentSelectedItem;
            }

            set
            {
                this.currentSelectedItem = value;
                this.SimulatorUpdated();
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            this.VisualizeCurrentState();
        }

        /// <summary>
        /// Creates the message animation.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        /// <param name="uimessage">The message control.</param>
        /// <param name="step">The step.</param>
        /// <returns>The created storyboard for the message animation.</returns>
        private static Storyboard CreateMessageAnimation(
            NetSimConnection edge,
            ConnectionFrameMessage message,
            MessageControl uimessage,
            NetSimMessageTransmissionStep step)
        {
            INetSimVisualizableItem receiver;
            INetSimVisualizableItem sender;
            GetMessageReceiverSender(edge, message, out receiver, out sender);

            int middleTop = (edge.EndPointA.Location.Top + edge.EndPointB.Location.Top) / 2;
            int middleLeft = (edge.EndPointA.Location.Left + edge.EndPointB.Location.Left) / 2;

            if (receiver == null || sender == null)
            {
                // return empty storyboard
                return new Storyboard();
            }

            DoubleAnimation animationTop = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.5) };
            DoubleAnimation animationLeft = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.5) };

            if (step == NetSimMessageTransmissionStep.Sending)
            {
                animationTop.From = sender.Location.Top;
                animationTop.To = middleTop;

                animationLeft.From = sender.Location.Left;
                animationLeft.To = middleLeft;
            }
            else
            {
                animationTop.From = middleTop;
                animationTop.To = receiver.Location.Top;

                animationLeft.From = middleLeft;
                animationLeft.To = receiver.Location.Left;
            }

            Storyboard board = new Storyboard();
            board.Children.Add(animationLeft);
            board.Children.Add(animationTop);

            Storyboard.SetTarget(animationTop, uimessage);
            Storyboard.SetTargetProperty(animationTop, new PropertyPath(Canvas.TopProperty));

            Storyboard.SetTarget(animationLeft, uimessage);
            Storyboard.SetTargetProperty(animationLeft, new PropertyPath(Canvas.LeftProperty));
            return board;
        }

        /// <summary>
        /// Gets the sender receiver.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="sender">The sender.</param>
        private static void GetMessageReceiverSender(
            NetSimConnection edge,
            ConnectionFrameMessage message,
            out INetSimVisualizableItem receiver,
            out INetSimVisualizableItem sender)
        {
            // receiver = (edge.EndPointA.Id.Equals(message.NextReceiver) ? edge.EndPointA : edge.EndPointB) as INetSimVisualizableItem;
            receiver =
                (edge.EndPointA.Id.Equals(message.Receiver) ? edge.EndPointA : edge.EndPointB) as
                    INetSimVisualizableItem;
            sender = ((receiver == edge.EndPointA) ? edge.EndPointB : edge.EndPointA) as INetSimVisualizableItem;
        }

        /// <summary>
        /// Adds the message.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        private void AddMessage(NetSimConnection edge, ConnectionFrameMessage message)
        {
            INetSimVisualizableItem receiver;
            INetSimVisualizableItem sender;

            // get message receiver and sender
            GetMessageReceiverSender(edge, message, out receiver, out sender);

            var uimessage = new MessageControl(message.ShortName)
            {
                Width = 19,
                Height = 19,
                Tag = edge,
                MessagePath = { Tag = edge }
            };

            // set message to sender location
            int top = sender.Location.Top;
            int left = sender.Location.Left;

            if (this.IsMessageSendingAnimationDone(message.Id))
            {
                // calculate middle between A and B
                top = (edge.EndPointA.Location.Top + edge.EndPointB.Location.Top) / 2;
                left = (edge.EndPointA.Location.Left + edge.EndPointB.Location.Left) / 2;
            }

            // add meesage to this position
            Canvas.SetLeft(uimessage, left);
            Canvas.SetTop(uimessage, top);

            this.drawCanvas.Children.Add(uimessage);

            Storyboard storyBoard = null;

            if (message.TransmissionStep == NetSimMessageTransmissionStep.Sending)
            {
                if (!this.IsMessageSendingAnimationDone(message.Id))
                {
                    storyBoard = CreateMessageAnimation(edge, message, uimessage, NetSimMessageTransmissionStep.Sending);

                    // messageStates[message.Id] = NetSimMessageTransmissionStep.Sending;
                }
            }
            else
            {
                if (!this.IsMessageReceivingAnimationDone(message.Id))
                {
                    storyBoard = CreateMessageAnimation(
                        edge,
                        message,
                        uimessage,
                        NetSimMessageTransmissionStep.Receiving);

                    // messageStates[message.Id] = NetSimMessageTransmissionStep.Receiving;
                }
            }

            if (storyBoard == null)
            {
                return;
            }

            // when the animation has finished - mark animation as done 
            // note: animation gets created multiple times - but only at last "redraw"
            // it has time to finish 
            storyBoard.Completed += (s, e) => { this.messageStates[message.Id] = message.TransmissionStep; };

            uimessage.BeginStoryboard(storyBoard);

            // start the animation
            storyBoard.Begin();
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The create element.</returns>
        private UIElement CreateAodvClientNode(NetSimClient node)
        {
            if (this.CurrentSelectedItem != null && node.Id.Equals(this.CurrentSelectedItem.Id))
            {
                return this.CreateClientNode(node, Brushes.LightSkyBlue);
            }

            return this.CreateClientNode(node, Brushes.CornflowerBlue);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="color">The color.</param>
        /// <param name="highlightBorder">if set to <c>true</c> highlight the border.</param>
        /// <returns>The create element for the node.</returns>
        private UIElement CreateClientNode(NetSimClient node, Brush color, bool highlightBorder = false)
        {
            var grid = new Grid { Tag = node };
            var ellipse = new Ellipse
            {
                Width = 50,
                Height = 50,
                Stroke = Brushes.Black,
                Fill = color,
                Tag = node,
                ToolTip = node.RoutingProtocol?.Table?.ToString()
            };

            if (node.IsOffline || highlightBorder)
            {
                ellipse.StrokeThickness = 4;

                ellipse.Stroke = highlightBorder ? Brushes.DarkOrange : Brushes.Red;
                ellipse.StrokeDashArray = new DoubleCollection(new List<double>() { 2, 2 });
            }

            var textBlock = new TextBlock
            {
                Text = node.Id,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = node
            };
            grid.Children.Add(ellipse);
            grid.Children.Add(textBlock);

            // To Center on click point
            Canvas.SetLeft(grid, node.Location.Left - 25);
            Canvas.SetTop(grid, node.Location.Top - 25);

            return grid;
        }

        /// <summary>
        /// Creates the connection edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <returns>The created element for the edge.</returns>
        private UIElement CreateConnectionEdge(NetSimConnection edge)
        {
            Line connectionLine = new Line
            {
                StrokeThickness = 4,
                StrokeDashOffset = 1,
                Stroke = Brushes.YellowGreen,
                Tag = edge,
                X1 = edge.EndPointA.Location.Left,
                Y1 = edge.EndPointA.Location.Top,
                X2 = edge.EndPointB.Location.Left,
                Y2 = edge.EndPointB.Location.Top,
                ToolTip = this.CreateConnectionTooltip(edge)
            };

            if (edge.IsOffline)
            {
                connectionLine.StrokeDashArray = new DoubleCollection(new List<double>() { 2, 2 });
                connectionLine.Stroke = Brushes.Red;
            }

            if (edge.IsTransmitting)
            {
                connectionLine.Stroke = Brushes.Orange;
            }

            return connectionLine;
        }

        /// <summary>
        /// Creates the connection tooltip.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <returns>The text for the connection tooltip.</returns>
        private string CreateConnectionTooltip(NetSimConnection edge)
        {
            if (edge == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (var message in edge.PendingMessages)
            {
                builder.AppendLine(message.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The created element for the dsdv node.</returns>
        private UIElement CreateDsdvClientNode(NetSimClient node)
        {
            if (this.CurrentSelectedItem != null && node.Id.Equals(this.CurrentSelectedItem.Id))
            {
                return this.CreateClientNode(node, Brushes.DarkSeaGreen);
            }

            return this.CreateClientNode(node, Brushes.YellowGreen);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The created element for the dsr node.</returns>
        private UIElement CreateDsrClientNode(NetSimClient node)
        {
            if (this.CurrentSelectedItem != null && node.Id.Equals(this.CurrentSelectedItem.Id))
            {
                return this.CreateClientNode(node, Brushes.LightSalmon);
            }

            return this.CreateClientNode(node, Brushes.OrangeRed);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The created element for the olsr node.</returns>
        private UIElement CreateOlsrClientNode(NetSimClient node)
        {
            if (this.CurrentSelectedItem != null)
            {
                if (node.Id.Equals(this.CurrentSelectedItem.Id))
                {
                    return this.CreateClientNode(node, Brushes.Thistle);
                }

                OlsrRoutingProtocol currentSelectedNodeProtocol =
                    this.CurrentSelectedItem.RoutingProtocol as OlsrRoutingProtocol;

                if (currentSelectedNodeProtocol != null)
                {
                    // is mpr neighbor
                    if (currentSelectedNodeProtocol.IsMprNeighbor(node.Id))
                    {
                        return this.CreateClientNode(node, Brushes.SteelBlue, true);
                    }

                    // is n(1 hop) neigbor
                    if (currentSelectedNodeProtocol.IsOneHopNeighbor(node.Id))
                    {
                        return this.CreateClientNode(node, Brushes.SteelBlue);
                    }

                    // is n(2 hop) neighbor
                    if (currentSelectedNodeProtocol.IsTwoHopNeighbor(node.Id))
                    {
                        return this.CreateClientNode(node, Brushes.PowderBlue);
                    }
                }
            }

            return this.CreateClientNode(node, Brushes.Orchid);
        }

        /// <summary>
        /// Creates the protocol specific client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The created element for the protocol specific node.</returns>
        private UIElement CreateProtocolSpecificClientNode(NetSimClient node)
        {
            if (node.RoutingProtocol is DsdvRoutingProtocol)
            {
                return this.CreateDsdvClientNode(node);
            }
            else if (node.RoutingProtocol is AodvRoutingProtocol)
            {
                return this.CreateAodvClientNode(node);
            }
            else if (node.RoutingProtocol is DsrRoutingProtocol)
            {
                return this.CreateDsrClientNode(node);
            }
            else if (node.RoutingProtocol is OlsrRoutingProtocol)
            {
                return this.CreateOlsrClientNode(node);
            }

            return null;
        }

        /// <summary>
        /// Determines whether if the message receiving animation
        /// is done for the specified message identifier.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>
        ///   <c>true</c> if the  message receiving animation is done
        ///   for the specified message identifier;
        ///   otherwise, <c>false</c>.
        /// </returns>
        private bool IsMessageReceivingAnimationDone(string messageId)
        {
            if (!this.messageStates.ContainsKey(messageId))
            {
                return false;
            }

            return this.messageStates[messageId] == NetSimMessageTransmissionStep.Receiving;
        }

        /// <summary>
        /// Determines whether if the message sending animation is done for the specified message identifier.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>
        ///    <c>true</c> if the  message sending animation is done
        ///   for the specified message identifier;
        ///   otherwise, <c>false</c>.
        /// </returns>
        private bool IsMessageSendingAnimationDone(string messageId)
        {
            if (!this.messageStates.ContainsKey(messageId))
            {
                return false;
            }

            return this.messageStates[messageId] == NetSimMessageTransmissionStep.Sending;
        }

        /// <summary>
        /// Simulators the updated.
        /// </summary>
        private void SimulatorUpdated()
        {
            this.VisualizeCurrentState();
        }

        /// <summary>
        /// Visualizes the current state of simulation.
        /// </summary>
        private void VisualizeCurrentState()
        {
            this.drawCanvas.Dispatcher.Invoke(
                () =>
                    {
                        this.drawCanvas.Children.Clear();

                        // draw the connections
                        foreach (NetSimConnection edge in this.simulator.Connections)
                        {
                            UIElement uiedge = this.CreateConnectionEdge(edge);

                            this.drawCanvas.Children.Add(uiedge);

                            if (!edge.IsOffline)
                            {
                                // create the end animation for transmitted messages that are in receiving step
                                foreach (
                                    var message in
                                    edge.TransmittedMessages.Where(
                                        m => m.TransmissionStep == NetSimMessageTransmissionStep.Receiving))
                                {
                                    this.AddMessage(edge, message);
                                }
                            }

                            if (edge.IsTransmitting && !edge.IsOffline)
                            {
                                // create the sending animation for transmitting messages
                                foreach (var message in edge.PendingMessages)
                                {
                                    this.AddMessage(edge, message);
                                }
                            }
                        }

                        // draw the clients
                        foreach (NetSimClient node in this.simulator.Clients)
                        {
                            UIElement uinode = this.CreateProtocolSpecificClientNode(node);

                            this.drawCanvas.Children.Add(uinode);
                        }
                    });
        }
    }
}