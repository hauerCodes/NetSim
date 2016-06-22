using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using NetSim.Lib.Controls;
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

        #region Constructor

        public NetSimVisualizer(IDrawableNetSimSimulator simulator, Canvas drawCanvas)
        {
            this.simulator = simulator;
            this.drawCanvas = drawCanvas;

            simulator.SimulatorUpdated += SimulatorUpdated;
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
            drawCanvas.Children.Clear();

            // draw the connections
            foreach (NetSimConnection edge in simulator.Connections)
            {
                UIElement uiedge = CreateConnectionEdge(edge);

                drawCanvas.Children.Add(uiedge);

                if (edge.IsTransmitting && !edge.IsOffline)
                {
                    UIElement message = CreateMessage(edge);

                    drawCanvas.Children.Add(message);
                }
            }

            // draw the clients
            foreach (NetSimClient node in simulator.Clients)
            {
                UIElement uinode = CreateClientNode(node);

                drawCanvas.Children.Add(uinode);
            }
        }

        private UIElement CreateMessage(NetSimConnection edge)
        {
            var message = new MessageControl { Width = 15, Height = 15, Tag = edge, MessagePath = { Tag = edge } };

            int top = (edge.EndPointA.Location.Top + edge.EndPointB.Location.Top) / 2;
            int left = (edge.EndPointA.Location.Left + edge.EndPointB.Location.Left) / 2;

            Canvas.SetLeft(message, left);
            Canvas.SetTop(message, top);

            return message;
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
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateClientNode(NetSimClient node)
        {
            var grid = new Grid { Tag = node };
            var ellipse = new Ellipse
            {
                Width = 50,
                Height = 50,
                Stroke = Brushes.Black,
                Fill = node.IsInitialized ? Brushes.YellowGreen : Brushes.LightGray,
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