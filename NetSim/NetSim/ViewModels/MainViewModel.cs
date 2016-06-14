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

using NetSim.DetailPages;
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

        private NetSimItem currentSelectedNode;

        private NetSimItem currentViewedItem;

        private Canvas drawCanvas;

        #region Constructor

        public MainViewModel(Canvas drawCanvas)
        {
            DrawCanvas = drawCanvas;
            Simulator = new NetSimSimulator();
            Visualizer = new NetSimVisualizer(Simulator, drawCanvas);
            this.nextNodeName = 'A';
            this.IsView = true;
        }

        #endregion

        #region Properties

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

        public NetSimItem CurrentSelectedNode
        {
            get
            {
                return currentSelectedNode;
            }
            set
            {
                currentSelectedNode = value;
                Debug.WriteLine(value != null ? $"CurrentSelected:{value.Id}" : $"CurrentSelected: -");

                RaisePropertyChanged();
            }
        }

        public NetSimItem CurrentViewedItem
        {
            get
            {
                return currentViewedItem;
            }
            set
            {
                currentViewedItem = value;
                Debug.WriteLine(value != null ? $"CurrentViewed:{value.Id}" : $"CurrentViewed: -");

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DetailPage));
            }
        }

        public Page DetailPage
        {
            get
            {
                if (currentViewedItem == null) return null;

                if(currentViewedItem is NetSimClient)
                {
                    return new ClientPage();
                }

                if (currentViewedItem is NetSimConnection)
                {
                    return new ConnectionPage();
                }

                return null;
            }
            
        }

        public Canvas DrawCanvas
        {
            get
            {
                return drawCanvas;
            }
            set
            {
                drawCanvas = value;
            }
        }

        public NetSimVisualizer Visualizer { get; set; }

        public NetSimSimulator Simulator { get; set; }

        #endregion

        #region Add Methods

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

        #endregion

        #region Handle Mouse Movement, Clicks

        public NetSimItem GetCurrentItem(Point getPosition)
        {
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

            if (Mouse.DirectlyOver is Line)
            {
                return (Mouse.DirectlyOver as Line).Tag as NetSimItem;
            }

            return null;
        }

        public void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var item = GetCurrentItem(mouseButtonEventArgs.GetPosition(DrawCanvas));

            if (item == null)
            {
                if (IsView)
                {
                    CurrentViewedItem = null;
                }

                return;
            }

            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (item is NetSimClient)
            {
                CurrentSelectedNode = item;

                switch (viewMode)
                {
                    case ViewMode.CreateEdges:
                        draftConnection = new NetSimConnection { From = (NetSimClient)item };
                        break;
                    case ViewMode.View:
                        CurrentViewedItem = item;
                        break;
                }
            }

            if (item is NetSimConnection)
            {
                if (IsView)
                {
                    CurrentViewedItem = item;
                }
            }
        }

        public void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (IsCreateNode && CurrentSelectedNode == null)
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

        public void HandleMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                switch (viewMode)
                {
                    case ViewMode.View:
                        if (CurrentSelectedNode != null)
                        {
                            var dragLocation = mouseEventArgs.GetPosition(DrawCanvas);
                            CurrentSelectedNode.Location.Left = (int)dragLocation.X;
                            CurrentSelectedNode.Location.Top = (int)dragLocation.Y;
                            Visualizer.Refresh();
                        }
                        else
                        {
                            CurrentSelectedNode = null;
                        }
                        break;
                    case ViewMode.CreateEdges:
                        if (draftConnection?.From != null)
                        {
                            var point = mouseEventArgs.GetPosition(DrawCanvas);

                            var node = GetCurrentItem(point);
                            if (node != null && node.Id != draftConnection.From.Id)
                            {
                                CurrentSelectedNode = node;
                                draftConnection.To = (NetSimClient)node;
                            }
                            else
                            {
                                CurrentSelectedNode = null;
                                draftConnection.To = null;
                            }

                            DrawConnectionLine(point, draftConnection.To != null);
                        }
                        else
                        {
                            CurrentSelectedNode = null;
                        }
                        break;
                }
            }
            else
            {
                CurrentSelectedNode = null;
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

        #endregion

    }
}
