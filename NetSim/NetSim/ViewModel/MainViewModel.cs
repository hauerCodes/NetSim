// -----------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - MainViewModel.cs</summary>
// -----------------------------------------------------------------------

// ReSharper disable ExplicitCallerInfoArgument
namespace NetSim.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using Microsoft.Win32;

    using NetSim.DetailPages;
    using NetSim.Lib.Simulator;
    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Storage;
    using NetSim.Lib.Visualization;

    /// <summary>
    /// The main view model.
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    [SuppressMessage("ReSharper", "TryCastAlwaysSucceeds", Justification = "Reason pattern feature not availiable in C# 6.0")]
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The simulation lock object
        /// </summary>
        private readonly object simulationLockObj = new object();

        /// <summary>
        /// The simulation step lock timespan
        /// </summary>
        private readonly TimeSpan simulationStepLockTimespan = TimeSpan.FromSeconds(0.25);

        /// <summary>
        /// The simulation step timespan
        /// </summary>
        private readonly TimeSpan simulationStepTimespan = TimeSpan.FromSeconds(0.55);

        /// <summary>
        /// The storage client
        /// </summary>
        private readonly SimulationStorage simulationStorage;

        /// <summary>
        /// The exit application command
        /// </summary>
        private ICommand closeApplicationCommand;

        /// <summary>
        /// The current selected node
        /// </summary>
        private NetSimItem currentSelectedNode;

        /// <summary>
        /// The current viewed item
        /// </summary>
        private NetSimItem currentViewedItem;

        /// <summary>
        /// The draft connection
        /// </summary>
        private NetSimConnection draftConnection;

        /// <summary>
        /// The draft connection line
        /// </summary>
        private Line draftConnectionLine;

        /// <summary>
        /// The is run simulation.
        /// </summary>
        private bool isRunSimulation;

        /// <summary>
        /// The flag that indicates if the  step button enabled
        /// </summary>
        private bool isStepEnabled;

        /// <summary>
        /// The load network command
        /// </summary>
        private ICommand loadNetworkCommand;

        /// <summary>
        /// The next node name
        /// </summary>
        private char nextNodeName = 'A';

        /// <summary>
        /// The pause simulation command
        /// </summary>
        private ICommand pauseSimulationCommand;

        /// <summary>
        /// The perform step command
        /// </summary>
        private ICommand performStepCommand;

        /// <summary>
        /// The protocol type
        /// </summary>
        private NetSimProtocolType protocolType;

        /// <summary>
        /// The reset simulation command
        /// </summary>
        private ICommand resetSimulationCommand;

        /// <summary>
        /// The save network command
        /// </summary>
        private ICommand saveNetworkCommand;

        /// <summary>
        /// The show about command
        /// </summary>
        private ICommand showAboutCommand;

        /// <summary>
        /// The show help command
        /// </summary>
        private ICommand showHelpCommand;

        /// <summary>
        /// The simulation task
        /// </summary>
        private Task simulationTask;

        /// <summary>
        /// The start simulation command
        /// </summary>
        private ICommand startSimulationCommand;

        /// <summary>
        /// The view mode
        /// </summary>
        private ViewMode viewMode;

        /// <summary>
        /// The window height
        /// </summary>
        private int windowHeight = 450;

        /// <summary>
        /// The window width
        /// </summary>
        private int windowWidth = 900;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="drawCanvas">The draw canvas.</param>
        public MainViewModel(Canvas drawCanvas)
        {
            this.DrawCanvas = drawCanvas;
            this.simulationStorage = new SimulationStorage();
            this.Simulator = new NetSimSimulator();
            this.Visualizer = new NetSimVisualizer(this.Simulator, drawCanvas);
            this.IsStepEnabled = true;
            this.Simulator.PropertyChanged += this.OnSimulatorPropertyChangedEventHandler;

            this.IsView = true;
            this.ProtocolType = NetSimProtocolType.DSR;

            this.InitializeCommands();
        }

        /// <summary>
        /// Gets or sets the close application command.
        /// </summary>
        /// <value>
        /// The close application command.
        /// </value>
        public ICommand CloseApplicationCommand
        {
            get
            {
                return this.closeApplicationCommand;
            }

            set
            {
                this.closeApplicationCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the control page.
        /// </summary>
        /// <value>
        /// The control page.
        /// </value>
        public Page ControlPage
        {
            get
            {
                if (this.currentViewedItem == null)
                {
                    return null;
                }

                if (this.currentViewedItem is NetSimClient)
                {
                    var controlClientPage = new ControlClientPage() { Client = this.currentViewedItem as NetSimClient };
                    controlClientPage.DeleteClientEvent += (sender, e) =>
                        {
                            this.Simulator.RemoveClient(e.Id);
                            this.Visualizer.Refresh();
                        };

                    return controlClientPage;
                }

                if (this.currentViewedItem is NetSimConnection)
                {
                    var controlConnectionPage = new ControlConnectionPage()
                    {
                        Connection = this.currentViewedItem as NetSimConnection
                    };
                    controlConnectionPage.DeleteConnectionEvent += (sender, e) =>
                        {
                            this.Simulator.RemoveConnection(e.EndPointA.Id, e.EndPointB.Id);
                            this.Visualizer.Refresh();
                        };

                    return controlConnectionPage;
                }

                return null;
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
                return this.currentSelectedNode;
            }

            set
            {
                this.currentSelectedNode = value;
                Debug.WriteLine(value != null ? $"CurrentSelected:{value.Id}" : $"CurrentSelected: -");

                this.RaisePropertyChanged();
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
                return this.currentViewedItem;
            }

            set
            {
                var oldValue = this.currentViewedItem;

                this.currentViewedItem = value;
                Debug.WriteLine(value != null ? $"CurrentViewed:{value.Id}" : $"CurrentViewed: -");

                var client = value as NetSimClient;
                if (client != null)
                {
                    this.Visualizer.CurrentSelectedItem = client;
                }

                // update every time to display new informations (e.g. messages, tables)
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.DetailPage));

                if (this.currentViewedItem == null || !this.currentViewedItem.Equals(oldValue))
                {
                    this.RaisePropertyChanged(nameof(this.ControlPage));
                }
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
                if (this.currentViewedItem == null)
                {
                    return null;
                }

                if (this.currentViewedItem is NetSimClient)
                {
                    return new ClientPage() { Client = this.currentViewedItem as NetSimClient };
                }

                if (this.currentViewedItem is NetSimConnection)
                {
                    return new ConnectionPage() { Connection = this.currentViewedItem as NetSimConnection };
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
        /// Gets or sets a value indicating whether this instance is create edge.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is create edge; otherwise, <c>false</c>.
        /// </value>
        public bool IsCreateEdge
        {
            get
            {
                return this.viewMode == ViewMode.CreateEdges;
            }

            set
            {
                if (value)
                {
                    this.viewMode = ViewMode.CreateEdges;
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.IsCreateNode));
                this.RaisePropertyChanged(nameof(this.IsView));
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
                return this.viewMode == ViewMode.CreateNodes;
            }

            set
            {
                if (value)
                {
                    this.viewMode = ViewMode.CreateNodes;
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.IsView));
                this.RaisePropertyChanged(nameof(this.IsCreateEdge));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the step button is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is step enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsStepEnabled
        {
            get
            {
                return this.isStepEnabled;
            }

            set
            {
                this.isStepEnabled = value;
                this.RaisePropertyChanged();
            }
        }

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
                return this.viewMode == ViewMode.View;
            }

            set
            {
                if (value)
                {
                    this.viewMode = ViewMode.View;
                }

                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.IsCreateNode));
                this.RaisePropertyChanged(nameof(this.IsCreateEdge));
            }
        }

        /// <summary>
        /// Gets or sets the load network command.
        /// </summary>
        /// <value>
        /// The load network command.
        /// </value>
        public ICommand LoadNetworkCommand
        {
            get
            {
                return this.loadNetworkCommand;
            }

            set
            {
                this.loadNetworkCommand = value;
                this.RaisePropertyChanged();
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
                return this.pauseSimulationCommand;
            }

            set
            {
                this.pauseSimulationCommand = value;
                this.RaisePropertyChanged();
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
                return this.performStepCommand;
            }

            set
            {
                this.performStepCommand = value;
                this.RaisePropertyChanged();
            }
        }

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
                this.RaisePropertyChanged();

                this.ExecuteResetSimulation();
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
                return this.resetSimulationCommand;
            }

            set
            {
                this.resetSimulationCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the save network command.
        /// </summary>
        /// <value>
        /// The save network command.
        /// </value>
        public ICommand SaveNetworkCommand
        {
            get
            {
                return this.saveNetworkCommand;
            }

            set
            {
                this.saveNetworkCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the show about command.
        /// </summary>
        /// <value>
        /// The show about command.
        /// </value>
        public ICommand ShowAboutCommand
        {
            get
            {
                return this.showAboutCommand;
            }

            set
            {
                this.showAboutCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the show help command.
        /// </summary>
        /// <value>
        /// The show help command.
        /// </value>
        public ICommand ShowHelpCommand
        {
            get
            {
                return this.showHelpCommand;
            }

            set
            {
                this.showHelpCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the simulation step.
        /// </summary>
        /// <value>
        /// The simulation step.
        /// </value>
        public int SimulationStep => this.Simulator.StepCounter;

        /// <summary>
        /// Gets or sets the simulator.
        /// </summary>
        /// <value>
        /// The simulator.
        /// </value>
        public NetSimSimulator Simulator { get; set; }

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
                return this.startSimulationCommand;
            }

            set
            {
                this.startSimulationCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the visualizer.
        /// </summary>
        /// <value>
        /// The visualizer.
        /// </value>
        public NetSimVisualizer Visualizer { get; set; }

        /// <summary>
        /// Gets or sets the Height of the window.
        /// </summary>
        /// <value>
        /// The Height of the window.
        /// </value>
        public int WindowHeight
        {
            get
            {
                return this.windowHeight;
            }

            set
            {
                this.windowHeight = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        /// <value>
        /// The width of the window.
        /// </value>
        public int WindowWidth
        {
            get
            {
                return this.windowWidth;
            }

            set
            {
                this.windowWidth = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Adds the edge.
        /// </summary>
        /// <param name="from">From item.</param>
        /// <param name="to">To item.</param>
        public void AddEdge(NetSimItem from, NetSimItem to)
        {
            try
            {
                lock (this.simulationLockObj)
                {
                    if (!this.Simulator.AddConnection(from.Id, to.Id, 1))
                    {
                        this.draftConnection = null;
                        this.DrawCanvas.Children.Remove(this.draftConnectionLine);
                        this.draftConnectionLine = null;
                    }
                }

                this.CheckCanExecuteCommands();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                this.draftConnection = null;
                this.DrawCanvas.Children.Remove(this.draftConnectionLine);
                this.draftConnectionLine = null;
            }
        }

        /// <summary>
        /// Adds the node.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The created node item.</returns>
        public NetSimItem AddNode(Point location)
        {
            NetSimItem returnObj;

            lock (this.simulationLockObj)
            {
                returnObj = this.Simulator.AddClient(this.nextNodeName.ToString(), (int)location.X, (int)location.Y);
            }

            this.nextNodeName++;

            this.CheckCanExecuteCommands();

            return returnObj;
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <param name="getPosition">The get position.</param>
        /// <returns>The current node item for this position.</returns>
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

            if (Mouse.DirectlyOver is Path)
            {
                return (Mouse.DirectlyOver as Path).Tag as NetSimItem;
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
            var item = this.GetCurrentItem(mouseButtonEventArgs.GetPosition(this.DrawCanvas));

            if (item == null)
            {
                if (this.IsView)
                {
                    this.CurrentViewedItem = null;
                }

                return;
            }

            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (item is NetSimClient)
            {
                this.CurrentSelectedNode = item;

                switch (this.viewMode)
                {
                    case ViewMode.CreateEdges:
                        this.draftConnection = new NetSimConnection { EndPointA = (NetSimClient)item };
                        break;
                    case ViewMode.View:
                        this.CurrentViewedItem = item;
                        break;
                }
            }

            if (item is NetSimConnection)
            {
                if (this.IsView)
                {
                    this.CurrentViewedItem = item;
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
            if (this.IsCreateNode && this.CurrentSelectedNode == null)
            {
                this.AddNode(mouseButtonEventArgs.GetPosition(this.DrawCanvas));
            }

            if (this.IsCreateEdge && this.draftConnection?.EndPointA != null && this.draftConnection?.EndPointB != null)
            {
                this.AddEdge(this.draftConnection.EndPointA as NetSimItem, this.draftConnection.EndPointB as NetSimItem);
                this.draftConnection = null;
            }
            else
            {
                this.draftConnection = null;
                this.DrawCanvas.Children.Remove(this.draftConnectionLine);
                this.draftConnectionLine = null;
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
                switch (this.viewMode)
                {
                    case ViewMode.View:
                        if (this.CurrentSelectedNode != null)
                        {
                            var dragLocation = mouseEventArgs.GetPosition(this.DrawCanvas);
                            this.CurrentSelectedNode.Location.Left = (int)dragLocation.X;
                            this.CurrentSelectedNode.Location.Top = (int)dragLocation.Y;
                            this.Visualizer.Refresh();
                        }
                        else
                        {
                            this.CurrentSelectedNode = null;
                        }

                        break;
                    case ViewMode.CreateEdges:
                        if (this.draftConnection?.EndPointA != null)
                        {
                            var point = mouseEventArgs.GetPosition(this.DrawCanvas);

                            var node = this.GetCurrentItem(point);
                            if (node != null && node.Id != this.draftConnection.EndPointA.Id && node is NetSimClient)
                            {
                                this.CurrentSelectedNode = node;
                                this.draftConnection.EndPointB = (NetSimClient)node;
                            }
                            else
                            {
                                this.CurrentSelectedNode = null;
                                this.draftConnection.EndPointB = null;
                            }

                            this.DrawConnectionLine(point, this.draftConnection.EndPointB != null);
                        }
                        else
                        {
                            this.CurrentSelectedNode = null;
                        }

                        break;
                }
            }
            else
            {
                this.CurrentSelectedNode = null;
            }
        }

        /// <summary>
        /// Determines whether this instance [can execute pause simulation].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute pause simulation]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecutePauseSimulation()
        {
            return this.SimulatorNetworkCreated() && this.isRunSimulation;
        }

        /// <summary>
        /// Determines whether this instance [can execute reset simulation].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute reset simulation]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteResetSimulation()
        {
            return this.SimulatorNetworkCreated();
        }

        /// <summary>
        /// Determines whether this instance [can execute simulation step].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute simulation step]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteSimulationStep()
        {
            return this.SimulatorNetworkCreated() && !this.isRunSimulation;
        }

        /// <summary>
        /// Determines whether this instance [can execute start simulation].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute start simulation]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteStartSimulation()
        {
            return this.SimulatorNetworkCreated() && !this.isRunSimulation;
        }

        /// <summary>
        /// Checks if commands can be executed.
        /// </summary>
        private void CheckCanExecuteCommands()
        {
            (this.PerformStepCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (this.StartSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (this.PauseSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (this.ResetSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Draws the connection line.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="possibleConnection">if set to <c>true</c> [possible connection].</param>
        private void DrawConnectionLine(Point endPoint, bool possibleConnection = false)
        {
            int drawOffset = 4;

            this.DrawCanvas.Children.Remove(this.draftConnectionLine);
            this.draftConnectionLine = null;

            if (this.draftConnection?.EndPointA != null)
            {
                this.draftConnectionLine = new Line()
                {
                    StrokeThickness = 2,
                    StrokeDashOffset = 1,
                    Stroke = possibleConnection ? Brushes.Green : Brushes.Red
                };
                this.DrawCanvas.Children.Add(this.draftConnectionLine);

                if (endPoint.X - this.draftConnection.EndPointA.Location.Left < 0
                    && endPoint.Y - this.draftConnection.EndPointA.Location.Top < 0)
                {
                    drawOffset = -4;
                }

                this.draftConnectionLine.X1 = this.draftConnection.EndPointA.Location.Left;
                this.draftConnectionLine.Y1 = this.draftConnection.EndPointA.Location.Top;
                this.draftConnectionLine.X2 = endPoint.X - drawOffset;
                this.draftConnectionLine.Y2 = endPoint.Y - drawOffset;
            }
        }

        /// <summary>
        /// Executes the close application.
        /// </summary>
        private void ExecuteCloseApplication()
        {
            Application.Current.MainWindow.Close();
        }

        /// <summary>
        /// Executes the load network.
        /// </summary>
        private void ExecuteLoadNetwork()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // InitialDirectory = @"c:\temp\",
                Filter = "netsim files(*.netsim)|*.netsim|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            var fallbackSimulator = this.Simulator;
            var fallbackVisualizer = this.Visualizer;

            try
            {
                this.ExecutePauseSimulation();

                var simulationInstance = this.simulationStorage.LoadNetwork(
                    openFileDialog.FileName,
                    (network) =>
                        {
                            var simulator = new NetSimSimulator();

                            foreach (var client in network.Clients)
                            {
                                simulator.AddClient(client.Id, client.Left, client.Top);
                            }

                            foreach (var connection in network.Connections)
                            {
                                simulator.AddConnection(connection.EndpointA, connection.EndpointB, connection.Metric);
                            }

                            return simulator;
                        });

                this.Simulator = (NetSimSimulator)simulationInstance;
                this.Visualizer = new NetSimVisualizer(this.Simulator, this.DrawCanvas);
                this.Simulator.PropertyChanged += this.OnSimulatorPropertyChangedEventHandler;

                // initialize simulator
                this.Simulator.InitializeProtocol(this.ProtocolType);

                if (this.Simulator.Clients.Count > 0)
                {
                    this.nextNodeName = this.Simulator.Clients.Max(c => c.Id)[0];
                    this.nextNodeName++;
                }
                else
                {
                    this.nextNodeName = 'A';
                }

                this.Visualizer.Refresh();
                this.CheckCanExecuteCommands();
            }
            catch (Exception ex)
            {
                this.Simulator = fallbackSimulator;
                this.Visualizer = fallbackVisualizer;

                Trace.TraceError(ex.Message);
            }
        }

        /// <summary>
        /// Executes the pause simulation.
        /// </summary>
        private void ExecutePauseSimulation()
        {
            lock (this.simulationLockObj)
            {
                this.isRunSimulation = false;
            }

            this.CheckCanExecuteCommands();
        }

        /// <summary>
        /// Executes the reset simulation.
        /// </summary>
        private void ExecuteResetSimulation()
        {
            lock (this.simulationLockObj)
            {
                this.isRunSimulation = false;
            }

            this.simulationTask?.Wait();

            this.Simulator.InitializeProtocol(this.ProtocolType);
            this.UpdateCurrentViewedItem();
            this.CheckCanExecuteCommands();
        }

        /// <summary>
        /// Executes the save network.
        /// </summary>
        private void ExecuteSaveNetwork()
        {
            SaveFileDialog openFileDialog = new SaveFileDialog
            {
                // InitialDirectory = @"c:\temp\",
                Filter = "netsim files(*.netsim)|*.netsim|All files (*.*)|*.*",
                DefaultExt = "netsim",
                FileName = $"NetworkSave_{DateTime.Now:ddMMyyyy}",
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                this.simulationStorage.SaveNetwork(openFileDialog.FileName, this.Simulator);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        /// <summary>
        /// Executes the show about.
        /// </summary>
        private void ExecuteShowAbout()
        {
            InfoWindow infoWindow = new InfoWindow();

            infoWindow.ShowDialog();
        }

        /// <summary>
        /// Executes the show help.
        /// </summary>
        private void ExecuteShowHelp()
        {
            HelpWindow helpWindow = new HelpWindow();

            helpWindow.ShowDialog();
        }

        /// <summary>
        /// Executes the simulation step.
        /// </summary>
        private void ExecuteSimulationStep()
        {
            // if step is currently executing - return
            if (!this.IsStepEnabled)
            {
                return;
            }

            // disable step button
            this.IsStepEnabled = false;

            if (!this.Simulator.IsInitialized)
            {
                this.Simulator.InitializeProtocol(this.ProtocolType);
            }

            this.Simulator.PerformSimulationStep();

            try
            {
                this.UpdateCurrentViewedItem();

                this.CheckCanExecuteCommands();
            }
            catch
            {
                // ignored
            }

            // start task - update enabled state of step button after time
            Task.Run(
                () =>
                    {
                        Thread.Sleep(this.simulationStepLockTimespan);
                        this.DrawCanvas.Dispatcher.Invoke(() => this.IsStepEnabled = true);
                    });
        }

        /// <summary>
        /// Executes the start simulation.
        /// </summary>
        private void ExecuteStartSimulation()
        {
            this.isRunSimulation = true;

            this.simulationTask = Task.Run(() => this.RunSimulation());

            this.CheckCanExecuteCommands();
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.PerformStepCommand = new RelayCommand(this.ExecuteSimulationStep, this.CanExecuteSimulationStep);
            this.StartSimulationCommand = new RelayCommand(this.ExecuteStartSimulation, this.CanExecuteStartSimulation);
            this.PauseSimulationCommand = new RelayCommand(this.ExecutePauseSimulation, this.CanExecutePauseSimulation);
            this.ResetSimulationCommand = new RelayCommand(this.ExecuteResetSimulation, this.CanExecuteResetSimulation);

            this.SaveNetworkCommand = new RelayCommand(this.ExecuteSaveNetwork);
            this.LoadNetworkCommand = new RelayCommand(this.ExecuteLoadNetwork);

            this.ShowHelpCommand = new RelayCommand(this.ExecuteShowHelp);
            this.ShowAboutCommand = new RelayCommand(this.ExecuteShowAbout);
            this.CloseApplicationCommand = new RelayCommand(this.ExecuteCloseApplication);
        }

        /// <summary>
        /// Called when a simulator property changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnSimulatorPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(this.Simulator.StepCounter)))
            {
                this.RaisePropertyChanged(nameof(this.SimulationStep));
            }
        }

        /// <summary>
        /// Runs the simulation.
        /// </summary>
        private void RunSimulation()
        {
            if (!this.Simulator.IsInitialized)
            {
                this.Simulator.InitializeProtocol(this.ProtocolType);
            }

            while (this.isRunSimulation)
            {
                lock (this.simulationLockObj)
                {
                    this.Simulator.PerformSimulationStep();
                }

                this.UpdateCurrentViewedItem();

                if (this.isRunSimulation)
                {
                    Thread.Sleep(this.simulationStepTimespan);
                }
            }
        }

        /// <summary>
        /// Checks if the simulators the network created.
        /// That means more than one client or more than one connection.
        /// </summary>
        /// <returns>true is the network is created; otherwise false.</returns>
        private bool SimulatorNetworkCreated()
        {
            return this.Simulator.Clients.Count > 0 && this.Simulator.Connections.Count > 0;
        }

        /// <summary>
        /// Updates the current viewed item.
        /// </summary>
        private void UpdateCurrentViewedItem()
        {
            if (this.CurrentViewedItem != null)
            {
                // update details view for client or connection
                this.CurrentViewedItem = this.CurrentViewedItem;
            }
        }
    }
}