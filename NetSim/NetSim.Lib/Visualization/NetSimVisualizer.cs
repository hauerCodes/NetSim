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
using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;
using NetSim.Lib.Simulator.Messages;

namespace NetSim.Lib.Visualization
{
    public class NetSimVisualizer
    {
        /// <summary>
        /// The simulator
        /// </summary>
        private readonly IDrawableNetSimSimulator simulator;

        /// <summary>
        /// The draw canvas
        /// </summary>
        private readonly Canvas drawCanvas;

        /// <summary>
        /// The message states
        /// </summary>
        private readonly Dictionary<string, NetSimMessageTransmissionStep> messageStates;

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

            simulator.SimulatorUpdated += SimulatorUpdated;
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
                return currentSelectedItem;
            }
            set
            {
                currentSelectedItem = value;
                SimulatorUpdated();
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            VisualizeCurrentState();
        }

        /// <summary>
        /// Simulators the updated.
        /// </summary>
        private void SimulatorUpdated()
        {
            VisualizeCurrentState();
        }

        /// <summary>
        /// Visualizes the current state of simulation.
        /// </summary>
        private void VisualizeCurrentState()
        {
            drawCanvas.Dispatcher.Invoke(() =>
            {
                drawCanvas.Children.Clear();

                // draw the connections
                foreach (NetSimConnection edge in simulator.Connections)
                {
                    UIElement uiedge = CreateConnectionEdge(edge);

                    drawCanvas.Children.Add(uiedge);

                    if (!edge.IsOffline)
                    {
                        // create the end animation for transmitted messages that are in receiving step
                        foreach (var message in edge.TransmittedMessages.Where(
                                    m => m.TransmissionStep == NetSimMessageTransmissionStep.Receiving))
                        {
                            AddMessage(edge, message);
                        }
                    }

                    if (edge.IsTransmitting && !edge.IsOffline)
                    {
                        // create the sending animation for transmitting messages
                        foreach (var message in edge.PendingMessages)
                        {
                            AddMessage(edge, message);
                        }
                    }
                }

                // draw the clients
                foreach (NetSimClient node in simulator.Clients)
                {
                    UIElement uinode = CreateProtocolSpecificClientNode(node);

                    drawCanvas.Children.Add(uinode);
                }
            });
        }

        /// <summary>
        /// Adds the message.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        private void AddMessage(NetSimConnection edge, ConnectionFrameMessage message)
        {
            INetSimVisualizeableItem receiver;
            INetSimVisualizeableItem sender;

            // get message receiver and sender
            GetMessageReceiverSender(edge, message, out receiver, out sender);

            var uimessage = new MessageControl { Width = 15, Height = 15, Tag = edge, MessagePath = { Tag = edge } };

            //set message to sender location
            int top = sender.Location.Top;
            int left = sender.Location.Left;

            if (IsMessageSendingAnimationDone(message.Id))
            {
                //calculate middle between A and B
                top = (edge.EndPointA.Location.Top + edge.EndPointB.Location.Top) / 2;
                left = (edge.EndPointA.Location.Left + edge.EndPointB.Location.Left) / 2;
            }

            // add meesage to this position
            Canvas.SetLeft(uimessage, left);
            Canvas.SetTop(uimessage, top);

            drawCanvas.Children.Add(uimessage);

            Storyboard storyBoard = null;

            if (message.TransmissionStep == NetSimMessageTransmissionStep.Sending)
            {
                if (!IsMessageSendingAnimationDone(message.Id))
                {
                    storyBoard = CreateMessageAnimation(edge, message, uimessage, NetSimMessageTransmissionStep.Sending);
                    //messageStates[message.Id] = NetSimMessageTransmissionStep.Sending;
                }
            }
            else
            {
                if (!IsMessageReceivingAnimationDone(message.Id))
                {
                    storyBoard = CreateMessageAnimation(edge, message, uimessage, NetSimMessageTransmissionStep.Receiving);
                    //messageStates[message.Id] = NetSimMessageTransmissionStep.Receiving;
                }
            }

            if (storyBoard == null) return;

            // when the animation has finished - mark animation as done 
            // note: animation gets created multiple times - but only at last "redraw"
            // it has time to finish 
            storyBoard.Completed += (s, e) =>
            {
                messageStates[message.Id] = message.TransmissionStep;
            };

            uimessage.BeginStoryboard(storyBoard);

            //start the animation
            storyBoard.Begin();

        }

        /// <summary>
        /// Gets the sender receiver.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="sender">The sender.</param>
        private static void GetMessageReceiverSender(NetSimConnection edge, ConnectionFrameMessage message, out INetSimVisualizeableItem receiver, out INetSimVisualizeableItem sender)
        {
            //receiver = (edge.EndPointA.Id.Equals(message.NextReceiver) ? edge.EndPointA : edge.EndPointB) as INetSimVisualizeableItem;
            receiver = (edge.EndPointA.Id.Equals(message.Receiver) ? edge.EndPointA : edge.EndPointB) as INetSimVisualizeableItem;
            sender = ((receiver == edge.EndPointA) ? edge.EndPointB : edge.EndPointA) as INetSimVisualizeableItem;
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
            if (!messageStates.ContainsKey(messageId))
            {
                return false;
            }

            return (messageStates[messageId] == NetSimMessageTransmissionStep.Receiving);
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
            if (!messageStates.ContainsKey(messageId))
            {
                return false;
            }

            return (messageStates[messageId] == NetSimMessageTransmissionStep.Sending);
        }

        /// <summary>
        /// Creates the message animation.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        /// <param name="uimessage">The uimessage.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        private static Storyboard CreateMessageAnimation(NetSimConnection edge, ConnectionFrameMessage message, MessageControl uimessage, NetSimMessageTransmissionStep step)
        {
            INetSimVisualizeableItem receiver;
            INetSimVisualizeableItem sender;
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
        /// Creates the connection edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <returns></returns>
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
                ToolTip = CreateConnectionTooltip(edge)
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
        /// <returns></returns>
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
        /// Creates the protocol specific client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateProtocolSpecificClientNode(NetSimClient node)
        {
            if (node.RoutingProtocol is DsdvRoutingProtocol)
            {
                return CreateDsdvClientNode(node);
            }
            else if (node.RoutingProtocol is AodvRoutingProtocol)
            {
                return CreateAodvClientNode(node);
            }
            else if (node.RoutingProtocol is DsrRoutingProtocol)
            {
                return CreateDsrClientNode(node);
            }
            else if (node.RoutingProtocol is OlsrRoutingProtocol)
            {
                return CreateOlsrClientNode(node);
            }
            return null;
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateDsdvClientNode(NetSimClient node)
        {
            if (CurrentSelectedItem != null && node.Id.Equals(CurrentSelectedItem.Id))
            {
                return CreateClientNode(node, Brushes.DarkSeaGreen);
            }

            return CreateClientNode(node, Brushes.YellowGreen);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateAodvClientNode(NetSimClient node)
        {
            if (CurrentSelectedItem != null && node.Id.Equals(CurrentSelectedItem.Id))
            {
                return CreateClientNode(node, Brushes.LightSkyBlue);
            }

            return CreateClientNode(node, Brushes.CornflowerBlue);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateDsrClientNode(NetSimClient node)
        {
            if (CurrentSelectedItem != null && node.Id.Equals(CurrentSelectedItem.Id))
            {
                return CreateClientNode(node, Brushes.LightSalmon);
            }

            return CreateClientNode(node, Brushes.OrangeRed);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateOlsrClientNode(NetSimClient node)
        {
            if (CurrentSelectedItem != null)
            {
                if (node.Id.Equals(CurrentSelectedItem.Id))
                {
                    return CreateClientNode(node, Brushes.Thistle);
                }

                OlsrRoutingProtocol currentSelectedNodeProtocol = CurrentSelectedItem.RoutingProtocol as OlsrRoutingProtocol;

                if (currentSelectedNodeProtocol != null)
                {
                    //is mpr neighbor
                    if (currentSelectedNodeProtocol.IsMprNeighbor(node.Id))
                    {
                        return CreateClientNode(node, Brushes.SteelBlue, true);
                    }

                    //is n(1 hop) neigbor
                    if (currentSelectedNodeProtocol.IsOneHopNeighbor(node.Id))
                    {
                        return CreateClientNode(node, Brushes.SteelBlue);
                    }

                    //is n(2 hop) neighbor
                    if (currentSelectedNodeProtocol.IsTwoHopNeighbor(node.Id))
                    {
                        return CreateClientNode(node, Brushes.PowderBlue);
                    }

                    
                }

            }

            return CreateClientNode(node, Brushes.Orchid);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="color">The color.</param>
        /// <param name="highlightBorder">if set to <c>true</c> highlight the border.</param>
        /// <returns></returns>
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

            //To Center on click point
            Canvas.SetLeft(grid, node.Location.Left - 25);
            Canvas.SetTop(grid, node.Location.Top - 25);

            return grid;
        }

    }

}