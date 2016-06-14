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
        private readonly IDrawableNetSimSimulator simulator;

        private readonly Canvas drawCanvas;

        public NetSimVisualizer(IDrawableNetSimSimulator simulator, Canvas drawCanvas)
        {
            this.simulator = simulator;
            this.drawCanvas = drawCanvas;

            simulator.Updated += SimulatorUpdated;
        }

        public void Refresh()
        {
            VisualizeCurrentState();
        }

        private void SimulatorUpdated()
        {
            VisualizeCurrentState();
        }

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

        private UIElement CreateConnectionEdge(NetSimConnection edge)
        {
            Line connectionLine = new Line() { StrokeThickness = 2, StrokeDashOffset = 1, Stroke = Brushes.YellowGreen };

            connectionLine.X1 = edge.From.Location.Left;
            connectionLine.Y1 = edge.From.Location.Top;
            connectionLine.X2 = edge.To.Location.Left;
            connectionLine.Y2 = edge.To.Location.Top;

            return connectionLine;
        }

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