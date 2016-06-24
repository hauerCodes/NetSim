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
        /// The current selected item
        /// </summary>
        private NetSimClient currentSelectedItem;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NetSimVisualizer"/> class.
        /// </summary>
        /// <param name="simulator">The simulator.</param>
        /// <param name="drawCanvas">The draw canvas.</param>
        public NetSimVisualizer(IDrawableNetSimSimulator simulator, Canvas drawCanvas)
        {
            this.simulator = simulator;
            this.drawCanvas = drawCanvas;

            simulator.SimulatorUpdated += SimulatorUpdated;
        }

        #endregion

        #region Properties

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

        #endregion

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
        /// Visualizes the state of the current.
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

                    if (edge.IsTransmitting && !edge.IsOffline)
                    {
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
        private void AddMessage(NetSimConnection edge, NetSimMessage message)
        {
            var uimessage = new MessageControl { Width = 15, Height = 15, Tag = edge, MessagePath = { Tag = edge } };

            int top = (edge.EndPointA.Location.Top + edge.EndPointB.Location.Top) / 2;
            int left = (edge.EndPointA.Location.Left + edge.EndPointB.Location.Left) / 2;

            Canvas.SetLeft(uimessage, left);
            Canvas.SetTop(uimessage, top);

            drawCanvas.Children.Add(uimessage);

            var storyBoard = CreateMessageAnimation(edge, message, uimessage);

            uimessage.BeginStoryboard(storyBoard);

            storyBoard.Begin();
        }

        /// <summary>
        /// Creates the message animation.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="message">The message.</param>
        /// <param name="uimessage">The uimessage.</param>
        /// <returns></returns>
        private static Storyboard CreateMessageAnimation(NetSimConnection edge, NetSimMessage message, MessageControl uimessage)
        {
            INetSimVisualizeableItem receiver = (edge.EndPointA.Id.Equals(message.NextReceiver) ? edge.EndPointA : edge.EndPointB) as INetSimVisualizeableItem;
            INetSimVisualizeableItem sender = (edge.EndPointA.Id.Equals(message.Sender) ? edge.EndPointA : edge.EndPointB) as INetSimVisualizeableItem;

            if (receiver == null || sender == null)
            {
                // return empty storyboard
                return new Storyboard();
            }

            DoubleAnimation animationTop = new DoubleAnimation()
            {
                From = sender.Location.Top,
                To = receiver.Location.Top,
                Duration = TimeSpan.FromSeconds(1)
            };

            DoubleAnimation animationLeft = new DoubleAnimation()
            {
                From = sender.Location.Left,
                To = receiver.Location.Left,
                Duration = TimeSpan.FromSeconds(1)
            };

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
                return null;
            }
            else if (node.RoutingProtocol is DsrRoutingProtocol)
            {
                return null;
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
        private UIElement CreateOlsrClientNode(NetSimClient node)
        {
            if (CurrentSelectedItem != null)
            {
                if (node.Id.Equals(CurrentSelectedItem.Id))
                {
                    return CreateClientNode(node, Brushes.MistyRose);
                }

                OlsrRoutingProtocol currentSelectedNodeProtocol = CurrentSelectedItem.RoutingProtocol as OlsrRoutingProtocol;

                if (currentSelectedNodeProtocol != null)
                {
                    //is n(1 hop) neigbor
                    if (currentSelectedNodeProtocol.IsOneHopNeighbor(node.Id))
                    {
                        return CreateClientNode(node, Brushes.DarkViolet);
                    }

                    //is n(2 hop) neighbor
                    if (currentSelectedNodeProtocol.IsTwoHopNeighbor(node.Id))
                    {
                        return CreateClientNode(node, Brushes.PaleVioletRed);
                    }

                    //is mpr neighbor
                }

            }

            return CreateClientNode(node, Brushes.Lavender);
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateClientNode(NetSimClient node, Brush color)
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