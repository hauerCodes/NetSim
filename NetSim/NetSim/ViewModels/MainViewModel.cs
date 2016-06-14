using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using GalaSoft.MvvmLight;

using NetSim.Lib.Simulator;
using NetSim.Lib.Visualization;
// ReSharper disable ExplicitCallerInfoArgument

namespace NetSim.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private char nextNodeName;

        private ViewMode viewMode;

        private NetSimConnection draftConnection;

        private Line draftConnectionLine;

        private NetSimItem currentSelected;

        public MainViewModel(Canvas drawCanvas)
        {
            DrawCanvas = drawCanvas;
            Simulator = new NetSimSimulator();
            Visualizer = new NetSimVisualizer(Simulator, drawCanvas);
            this.nextNodeName = 'A';
            this.IsView = true;
        }

        public bool IsView
        {
            get
            {
                return viewMode == ViewMode.View;
            }
            set
            {
                if (value)
                {
                    viewMode = ViewMode.View;
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsCreateNode));
                RaisePropertyChanged(nameof(IsCreateEdge));
            }
        }

        public bool IsCreateNode
        {
            get
            {
                return viewMode == ViewMode.CreateNodes;
            }
            set
            {
                if (value)
                {
                    viewMode = ViewMode.CreateNodes;
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsView));
                RaisePropertyChanged(nameof(IsCreateEdge));
            }
        }

        public bool IsCreateEdge
        {
            get
            {
                return viewMode == ViewMode.CreateEdges;
            }
            set
            {
                if (value)
                {
                    viewMode = ViewMode.CreateEdges;
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsCreateNode));
                RaisePropertyChanged(nameof(IsView));
            }
        }

        public NetSimItem CurrentSelected
        {
            get
            {
                return currentSelected;
            }
            set
            {
                currentSelected = value;
                if (value != null)
                {
                    Debug.WriteLine($"CurrentSelected:{value.Id}");
                }
                else
                {
                    Debug.WriteLine($"CurrentSelected: -");
                }
            }
        }

        public Canvas DrawCanvas { get; set; }

        public NetSimVisualizer Visualizer { get; set; }

        public NetSimSimulator Simulator { get; set; }

        public NetSimItem AddNode(Point location)
        {
            var returnObj = Simulator.AddClient(nextNodeName.ToString(), (int)location.X, (int)location.Y);

            nextNodeName++;

            return returnObj;
        }

        public void AddEdge(NetSimItem from, NetSimItem to)
        {
            try
            {
                Simulator.AddConnection(from.Id, to.Id, 1);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                draftConnection = null;
                DrawCanvas.Children.Remove(draftConnectionLine);
                draftConnectionLine = null;
            }
        }

        public NetSimItem GetNode(Point getPosition)
        {
            //if(Mouse.DirectlyOver == null || Mouse.DirectlyOver == DrawCanvas)
            //{
            //    return null;
            //}

            Debug.WriteLine(Mouse.DirectlyOver);

            if (Mouse.DirectlyOver is Rectangle)
            {
                return (Mouse.DirectlyOver as Rectangle).Tag as NetSimItem;
            }
            if (Mouse.DirectlyOver is Ellipse)
            {
                return (Mouse.DirectlyOver as Ellipse).Tag as NetSimItem;
            }
            if (Mouse.DirectlyOver is Grid)
            {
                return (Mouse.DirectlyOver as Grid).Tag as NetSimItem;
            }
            if (Mouse.DirectlyOver is TextBlock)
            {
                return (Mouse.DirectlyOver as TextBlock).Tag as NetSimItem;
            }
            return null;
        }

        public void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (IsCreateNode && CurrentSelected == null)
            {
                AddNode(mouseButtonEventArgs.GetPosition(DrawCanvas));
            }

            if (IsCreateEdge && draftConnection?.From != null && draftConnection?.To != null)
            {
                AddEdge(draftConnection.From, draftConnection.To);
                draftConnection = null;
            }
            else
            {
                draftConnection = null;
                DrawCanvas.Children.Remove(draftConnectionLine);
                draftConnectionLine = null;
            }
        }

        public void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Debug.WriteLine(nameof(HandleMouseLeftButtonDown));
            var node = GetNode(mouseButtonEventArgs.GetPosition(DrawCanvas));
            if (node is NetSimClient)
            {
                CurrentSelected = node;

                if (IsCreateEdge)
                {
                    draftConnection = new NetSimConnection();
                    draftConnection.From = (NetSimClient)node;
                }
            }
        }

        private void DrawConnectionLine(Point endPoint, bool possibleConnection = false)
        {
            int drawOffset = 4;

            DrawCanvas.Children.Remove(draftConnectionLine);
            draftConnectionLine = null;

            if (draftConnection?.From != null)
            {
                draftConnectionLine = new Line() { StrokeThickness = 2, StrokeDashOffset = 1, Stroke = possibleConnection ? Brushes.Green : Brushes.Red };
                DrawCanvas.Children.Add(draftConnectionLine);

                if (endPoint.X - draftConnection.From.Location.Left < 0 &&
                   endPoint.Y - draftConnection.From.Location.Top < 0)
                {
                    drawOffset = -4;
                }

                draftConnectionLine.X1 = draftConnection.From.Location.Left;
                draftConnectionLine.Y1 = draftConnection.From.Location.Top;
                draftConnectionLine.X2 = endPoint.X - drawOffset;
                draftConnectionLine.Y2 = endPoint.Y - drawOffset;
            }
        }

        public void HandleMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                switch (viewMode)
                {
                    case ViewMode.View:
                        if (CurrentSelected != null)
                        {
                            var dragLocation = mouseEventArgs.GetPosition(DrawCanvas);
                            CurrentSelected.Location.Left = (int)dragLocation.X;
                            CurrentSelected.Location.Top = (int)dragLocation.Y;
                            Visualizer.Refresh();
                        }
                        else
                        {
                            CurrentSelected = null;
                        }
                        break;
                    case ViewMode.CreateEdges:
                        if (draftConnection?.From != null)
                        {
                            var point = mouseEventArgs.GetPosition(DrawCanvas);

                            var node = GetNode(point);
                            if (node != null && node.Id != draftConnection.From.Id)
                            {
                                CurrentSelected = node;
                                draftConnection.To = (NetSimClient)node;
                            }
                            else
                            {
                                CurrentSelected = null;
                                draftConnection.To = null;
                            }

                            DrawConnectionLine(point, draftConnection.To != null);
                        }
                        else
                        {
                            CurrentSelected = null;
                        }
                        break;
                }
            }
            else
            {
                CurrentSelected = null;
            }
        }
    }
}
