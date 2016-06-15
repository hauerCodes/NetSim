using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

            foreach (NetSimConnection edge in simulator.Connections)
            {
                UIElement uiedge = CreateConnectionEdge(edge);

                drawCanvas.Children.Add(uiedge);
            }

            foreach (NetSimClient node in simulator.Clients)
            {
                UIElement uinode = CreateClientNode(node);

                drawCanvas.Children.Add(uinode);
            }
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
                X1 = edge.From.Location.Left,
                Y1 = edge.From.Location.Top,
                X2 = edge.To.Location.Left,
                Y2 = edge.To.Location.Top
            };


            return connectionLine;
        }

        /// <summary>
        /// Creates the client node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private UIElement CreateClientNode(NetSimItem node)
        {
            var grid = new Grid { Tag = node };
            var ellipse = new Ellipse
            {
                Width = 50,
                Height = 50,
                Stroke = Brushes.Black,
                Fill = Brushes.LightGray,
                Tag = node
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