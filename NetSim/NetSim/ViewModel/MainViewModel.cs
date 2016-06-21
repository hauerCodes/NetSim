using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using GalaSoft.MvvmLight.Command;

using NetSim.DetailPages;
using NetSim.Lib.Simulator;
using NetSim.Lib.Visualization;
// ReSharper disable ExplicitCallerInfoArgument

namespace NetSim.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private char nextNodeName = 'A';

        private ViewMode viewMode;

        private NetSimProtocolType protocolType;

        private NetSimConnection draftConnection;

        private Line draftConnectionLine;

        private NetSimItem currentSelectedNode;

        private NetSimItem currentViewedItem;

        private ICommand startSimulationCommand;

        private ICommand pauseSimulationCommand;

        private ICommand performStepCommand;

        private ICommand resetSimulationCommand;

        #region Constructor

        public MainViewModel(Canvas drawCanvas)
        {
            this.DrawCanvas = drawCanvas;
            this.Simulator = new NetSimSimulator();
            this.Visualizer = new NetSimVisualizer(Simulator, drawCanvas);

            this.Simulator.PropertyChanged += OnSimulatorPropertyChangedEventHandler;

            this.IsView = true;
            this.ProtocolType = NetSimProtocolType.DSDV;

            InitializeCommands();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is view; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is create node.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is create node; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is create edge.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is create edge; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets the current selected node.
        /// </summary>
        /// <value>
        /// The current selected node.
        /// </value>
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

        /// <summary>
        /// Gets or sets the current viewed item.
        /// </summary>
        /// <value>
        /// The current viewed item.
        /// </value>
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

        /// <summary>
        /// Gets the detail page.
        /// </summary>
        /// <value>
        /// The detail page.
        /// </value>
        public Page DetailPage
        {
            get
            {
                if (currentViewedItem == null) return null;

                if (currentViewedItem is NetSimClient)
                {
                    return new ClientPage() { Client = currentViewedItem as NetSimClient };
                }

                if (currentViewedItem is NetSimConnection)
                {
                    return new ConnectionPage() { Connection = currentViewedItem as NetSimConnection };
                }

                return null;
            }

        }

        /// <summary>
        /// Gets or sets the draw canvas.
        /// </summary>
        /// <value>
        /// The draw canvas.
        /// </value>
        public Canvas DrawCanvas { get; set; }

        /// <summary>
        /// Gets or sets the visualizer.
        /// </summary>
        /// <value>
        /// The visualizer.
        /// </value>
        public NetSimVisualizer Visualizer { get; set; }

        /// <summary>
        /// Gets or sets the simulator.
        /// </summary>
        /// <value>
        /// The simulator.
        /// </value>
        public NetSimSimulator Simulator { get; set; }

        /// <summary>
        /// Gets or sets the type of the protocol.
        /// </summary>
        /// <value>
        /// The type of the protocol.
        /// </value>
        public NetSimProtocolType ProtocolType
        {
            get
            {
                return this.protocolType;
            }
            set
            {
                this.protocolType = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the simulation step.
        /// </summary>
        /// <value>
        /// The simulation step.
        /// </value>
        public int SimulationStep => this.Simulator.StepCounter;

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the start simulation command.
        /// </summary>
        /// <value>
        /// The start simulation command.
        /// </value>
        public ICommand StartSimulationCommand
        {
            get
            {
                return startSimulationCommand;
            }
            set
            {
                this.startSimulationCommand = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the start simulation command.
        /// </summary>
        /// <value>
        /// The start simulation command.
        /// </value>
        public ICommand PauseSimulationCommand
        {
            get
            {
                return pauseSimulationCommand;
            }
            set
            {
                this.pauseSimulationCommand = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the start simulation command.
        /// </summary>
        /// <value>
        /// The start simulation command.
        /// </value>
        public ICommand ResetSimulationCommand
        {
            get
            {
                return resetSimulationCommand;
            }
            set
            {
                this.resetSimulationCommand = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the perform step command.
        /// </summary>
        /// <value>
        /// The perform step command.
        /// </value>
        public ICommand PerformStepCommand
        {
            get
            {
                return performStepCommand;
            }
            set
            {
                this.performStepCommand = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Initialize
        private void InitializeCommands()
        {
            this.PerformStepCommand = new RelayCommand(ExecuteSimulationStep, CanExecuteSimulationStep);
            this.StartSimulationCommand = new RelayCommand(ExecuteStartSimulation, CanExecuteStartSimulation);
            this.PauseSimulationCommand = new RelayCommand(ExecutePauseSimulation, CanExecutePauseSimulation);
            this.ResetSimulationCommand = new RelayCommand(ExecutePauseSimulation, CanExecutePauseSimulation);
        }

        #endregion

        #region Command Execution

        private void ExecutePauseSimulation()
        {
            throw new NotImplementedException();
        }

        private void ExecuteSimulationStep()
        {
            if (!Simulator.IsInitialized)
            {
                Simulator.InitializeProtocol(NetSimProtocolType.DSDV);
            }

            Simulator.PerformSimulationStep();

            if(CurrentViewedItem != null)
            {
                // update details view for client or connection
                this.CurrentViewedItem = CurrentViewedItem;
            }
        }

        private void ExecuteStartSimulation()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Command CanExecute Check

        private bool CanExecutePauseSimulation()
        {
            return SimulatorNetworkCreated();
        }

        private bool CanExecuteStartSimulation()
        {
            return SimulatorNetworkCreated();
        }

        private bool CanExecuteSimulationStep()
        {
            return SimulatorNetworkCreated();
        }

        private bool SimulatorNetworkCreated()
        {
            return Simulator.Clients.Count > 0 && Simulator.Connections.Count > 0;
        }


        #endregion

        #region Add Methods

        /// <summary>
        /// Adds the node.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public NetSimItem AddNode(Point location)
        {
            var returnObj = Simulator.AddClient(nextNodeName.ToString(), (int)location.X, (int)location.Y);

            nextNodeName++;

            CheckCanExecuteCommands();

            return returnObj;
        }

        /// <summary>
        /// Adds the edge.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public void AddEdge(NetSimItem from, NetSimItem to)
        {
            try
            {
                if (!Simulator.AddConnection(from.Id, to.Id, 1))
                {
                    draftConnection = null;
                    DrawCanvas.Children.Remove(draftConnectionLine);
                    draftConnectionLine = null;
                }

                CheckCanExecuteCommands();

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

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <param name="getPosition">The get position.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="mouseButtonEventArgs">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
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
                        draftConnection = new NetSimConnection { EndPointA = (NetSimClient)item };
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

        /// <summary>
        /// Handles the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="mouseButtonEventArgs">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        public void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (IsCreateNode && CurrentSelectedNode == null)
            {
                AddNode(mouseButtonEventArgs.GetPosition(DrawCanvas));
            }

            if (IsCreateEdge && draftConnection?.EndPointA != null && draftConnection?.EndPointB != null)
            {
                AddEdge(draftConnection.EndPointA, draftConnection.EndPointB);
                draftConnection = null;
            }
            else
            {
                draftConnection = null;
                DrawCanvas.Children.Remove(draftConnectionLine);
                draftConnectionLine = null;
            }
        }

        /// <summary>
        /// Handles the mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="mouseEventArgs">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
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
                        if (draftConnection?.EndPointA != null)
                        {
                            var point = mouseEventArgs.GetPosition(DrawCanvas);

                            var node = GetCurrentItem(point);
                            if (node != null && node.Id != draftConnection.EndPointA.Id && node is NetSimClient)
                            {
                                CurrentSelectedNode = node;
                                draftConnection.EndPointB = (NetSimClient)node;
                            }
                            else
                            {
                                CurrentSelectedNode = null;
                                draftConnection.EndPointB = null;
                            }

                            DrawConnectionLine(point, draftConnection.EndPointB != null);
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

        /// <summary>
        /// Draws the connection line.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="possibleConnection">if set to <c>true</c> [possible connection].</param>
        private void DrawConnectionLine(Point endPoint, bool possibleConnection = false)
        {
            int drawOffset = 4;

            DrawCanvas.Children.Remove(draftConnectionLine);
            draftConnectionLine = null;

            if (draftConnection?.EndPointA != null)
            {
                draftConnectionLine = new Line() { StrokeThickness = 2, StrokeDashOffset = 1, Stroke = possibleConnection ? Brushes.Green : Brushes.Red };
                DrawCanvas.Children.Add(draftConnectionLine);

                if (endPoint.X - draftConnection.EndPointA.Location.Left < 0 &&
                   endPoint.Y - draftConnection.EndPointA.Location.Top < 0)
                {
                    drawOffset = -4;
                }

                draftConnectionLine.X1 = draftConnection.EndPointA.Location.Left;
                draftConnectionLine.Y1 = draftConnection.EndPointA.Location.Top;
                draftConnectionLine.X2 = endPoint.X - drawOffset;
                draftConnectionLine.Y2 = endPoint.Y - drawOffset;
            }
        }

        #endregion

        /// <summary>
        /// Called when a simulator property changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnSimulatorPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Simulator.StepCounter)))
            {
                RaisePropertyChanged(nameof(SimulationStep));
            }
        }

        /// <summary>
        /// Checks if commands can be executed.
        /// </summary>
        private void CheckCanExecuteCommands()
        {
            (PerformStepCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StartSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PauseSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ResetSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

    }
}
