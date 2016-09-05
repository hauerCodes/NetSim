using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
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
// ReSharper disable ExplicitCallerInfoArgument

namespace NetSim.ViewModel
{
    [SuppressMessage("ReSharper", "TryCastAlwaysSucceeds")]
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The next node name
        /// </summary>
        private char nextNodeName = 'A';

        /// <summary>
        /// The view mode
        /// </summary>
        private ViewMode viewMode;

        /// <summary>
        /// The is run simluation
        /// </summary>
        private bool isRunSimluation;

        /// <summary>
        /// The simulation step timespan
        /// </summary>
        private TimeSpan simulationStepTimespan = TimeSpan.FromSeconds(0.55);

        /// <summary>
        /// The storage client
        /// </summary>
        private readonly SimulationStorage simulationStorage;

        /// <summary>
        /// The simluation lock object
        /// </summary>
        private readonly object simluationLockObj = new object();

        /// <summary>
        /// The protocol type
        /// </summary>
        private NetSimProtocolType protocolType;

        /// <summary>
        /// The draft connection
        /// </summary>
        private NetSimConnection draftConnection;

        /// <summary>
        /// The draft connection line
        /// </summary>
        private Line draftConnectionLine;

        /// <summary>
        /// The current selected node
        /// </summary>
        private NetSimItem currentSelectedNode;

        /// <summary>
        /// The current viewed item
        /// </summary>
        private NetSimItem currentViewedItem;

        /// <summary>
        /// The save network command
        /// </summary>
        private ICommand saveNetworkCommand;

        /// <summary>
        /// The load network command
        /// </summary>
        private ICommand loadNetworkCommand;

        /// <summary>
        /// The start simulation command
        /// </summary>
        private ICommand startSimulationCommand;

        /// <summary>
        /// The pause simulation command
        /// </summary>
        private ICommand pauseSimulationCommand;

        /// <summary>
        /// The show about command
        /// </summary>
        private ICommand showAboutCommand;

        /// <summary>
        /// The show help command
        /// </summary>
        private ICommand showHelpCommand;

        /// <summary>
        /// The exit application command
        /// </summary>
        private ICommand closeApplicationCommand;

        /// <summary>
        /// The perform step command
        /// </summary>
        private ICommand performStepCommand;

        /// <summary>
        /// The reset simulation command
        /// </summary>
        private ICommand resetSimulationCommand;

        /// <summary>
        /// The simulation task
        /// </summary>
        private Task simulationTask;

        /// <summary>
        /// The flag that indicates if the  step button enabled
        /// </summary>
        private bool isStepEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="drawCanvas">The draw canvas.</param>
        public MainViewModel(Canvas drawCanvas)
        {
            this.DrawCanvas = drawCanvas;
            this.simulationStorage = new SimulationStorage();
            this.Simulator = new NetSimSimulator();
            this.Visualizer = new NetSimVisualizer(Simulator, drawCanvas);
            this.IsStepEnabled = true;
            this.Simulator.PropertyChanged += OnSimulatorPropertyChangedEventHandler;

            this.IsView = true;
            this.ProtocolType = NetSimProtocolType.DSR;

            InitializeCommands();
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
        /// Gets or sets a value indicating whether the step button is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is step enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsStepEnabled
        {
            get
            {
                return isStepEnabled;
            }
            set
            {
                isStepEnabled = value;
                RaisePropertyChanged();
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
                var oldValue = currentViewedItem;

                currentViewedItem = value;
                Debug.WriteLine(value != null ? $"CurrentViewed:{value.Id}" : $"CurrentViewed: -");

                var client = value as NetSimClient;
                if (client != null)
                {
                    this.Visualizer.CurrentSelectedItem = client;
                }

                // update every time to display new informations (e.g. messages, tables)
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DetailPage));


                if (currentViewedItem == null || !currentViewedItem.Equals(oldValue))
                {
                    RaisePropertyChanged(nameof(ControlPage));
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
        /// Gets the control page.
        /// </summary>
        /// <value>
        /// The control page.
        /// </value>
        public Page ControlPage
        {
            get
            {
                if (currentViewedItem == null) return null;

                if (currentViewedItem is NetSimClient)
                {
                    var controlClientPage = new ControlClientPage() { Client = currentViewedItem as NetSimClient };
                    controlClientPage.DeleteClientEvent += (sender, e) =>
                    {
                        Simulator.RemoveClient(e.Id);
                        Visualizer.Refresh();
                    };

                    return controlClientPage;

                }

                if (currentViewedItem is NetSimConnection)
                {
                    var controlConnectionPage = new ControlConnectionPage() { Connection = currentViewedItem as NetSimConnection };
                    controlConnectionPage.DeleteConnectionEvent +=
                        (sender, e) =>
                        {
                            Simulator.RemoveConnection(e.EndPointA.Id, e.EndPointB.Id);
                            Visualizer.Refresh();
                        };

                    return controlConnectionPage;
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

                ExecuteResetSimulation();

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
                return saveNetworkCommand;
            }
            set
            {
                this.saveNetworkCommand = value;
                RaisePropertyChanged();
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
                return loadNetworkCommand;
            }
            set
            {
                this.loadNetworkCommand = value;
                RaisePropertyChanged();
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
                return showHelpCommand;
            }
            set
            {
                this.showHelpCommand = value;
                RaisePropertyChanged();
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
                return showAboutCommand;
            }
            set
            {
                this.showAboutCommand = value;
                RaisePropertyChanged();
            }
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
                return closeApplicationCommand;
            }
            set
            {
                this.closeApplicationCommand = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.PerformStepCommand = new RelayCommand(ExecuteSimulationStep, CanExecuteSimulationStep);     
            this.StartSimulationCommand = new RelayCommand(ExecuteStartSimulation, CanExecuteStartSimulation);          
            this.PauseSimulationCommand = new RelayCommand(ExecutePauseSimulation, CanExecutePauseSimulation);
            this.ResetSimulationCommand = new RelayCommand(ExecuteResetSimulation, CanExecuteResetSimulation);
            
            this.SaveNetworkCommand = new RelayCommand(ExecuteSaveNetwork);
            this.LoadNetworkCommand = new RelayCommand(ExecuteLoadNetwork);

            this.ShowHelpCommand = new RelayCommand(ExecuteShowHelp);
            this.ShowAboutCommand = new RelayCommand(ExecuteShowAbout);
            this.CloseApplicationCommand = new RelayCommand(ExecuteCloseApplication);
        }

        /// <summary>
        /// Executes the close application.
        /// </summary>
        private void ExecuteCloseApplication()
        {
            Application.Current.MainWindow.Close();
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
        /// Executes the show about.
        /// </summary>
        private void ExecuteShowAbout()
        {
            InfoWindow infoWindow = new InfoWindow();

            infoWindow.ShowDialog();
        }

        /// <summary>
        /// Executes the load network.
        /// </summary>
        private void ExecuteLoadNetwork()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"c:\temp\",
                Filter = "netsim files(*.netsim)|*.netsim|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true) return;

            var fallbackSimulator = this.Simulator;
            var fallbackVisualizer = this.Visualizer;

            try
            {
                ExecutePauseSimulation();

                var simulationInstance = simulationStorage.LoadNetwork(openFileDialog.FileName,
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
                this.Visualizer = new NetSimVisualizer(Simulator, DrawCanvas);
                this.Simulator.PropertyChanged += OnSimulatorPropertyChangedEventHandler;

                // initialize simulator
                this.Simulator.InitializeProtocol(ProtocolType);

                if (Simulator.Clients.Count > 0)
                {
                    this.nextNodeName = Simulator.Clients.Max(c => c.Id)[0];
                    this.nextNodeName++;
                }
                else
                {
                    this.nextNodeName = 'A';
                }

                Visualizer.Refresh();
                CheckCanExecuteCommands();
            }
            catch (Exception ex)
            {
                this.Simulator = fallbackSimulator;
                this.Visualizer = fallbackVisualizer;

                Trace.TraceError(ex.Message);
            }
        }

        /// <summary>
        /// Executes the save network.
        /// </summary>
        private void ExecuteSaveNetwork()
        {
            SaveFileDialog openFileDialog = new SaveFileDialog
            {
                InitialDirectory = @"c:\temp\",
                Filter = "netsim files(*.netsim)|*.netsim|All files (*.*)|*.*",
                DefaultExt = "netsim",
                FileName = $"NetworkSave_{DateTime.Now.ToString("ddMMyyyy")}",
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                simulationStorage.SaveNetwork(openFileDialog.FileName, Simulator);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        /// <summary>
        /// Executes the start simulation.
        /// </summary>
        private void ExecuteStartSimulation()
        {
            isRunSimluation = true;

            simulationTask = Task.Run(() => RunSimulation());

            CheckCanExecuteCommands();
        }

        /// <summary>
        /// Executes the pause simulation.
        /// </summary>
        private void ExecutePauseSimulation()
        {
            lock (simluationLockObj)
            {
                isRunSimluation = false;
            }

            CheckCanExecuteCommands();
        }

        /// <summary>
        /// Executes the simulation step.
        /// </summary>
        private void ExecuteSimulationStep()
        {
            //disable step button
            IsStepEnabled = false;

            if (!Simulator.IsInitialized)
            {
                Simulator.InitializeProtocol(ProtocolType);
            }

            Simulator.PerformSimulationStep();

            try
            {
                UpdateCurrentViewedItem();

                CheckCanExecuteCommands();
            }
            catch
            {
                //ignored
            }

            // start task - update enabled state of step button after time
            Task.Run(() =>
            {
                Thread.Sleep(simulationStepTimespan);
                DrawCanvas.Dispatcher.Invoke(() => IsStepEnabled = true);
            });
        }

        /// <summary>
        /// Runs the simulation.
        /// </summary>
        private void RunSimulation()
        {
            if (!Simulator.IsInitialized)
            {
                Simulator.InitializeProtocol(ProtocolType);
            }

            while (isRunSimluation)
            {
                lock (simluationLockObj)
                {
                    Simulator.PerformSimulationStep();
                }

                UpdateCurrentViewedItem();

                if (isRunSimluation)
                {
                    Thread.Sleep(simulationStepTimespan);
                }
            }
        }

        /// <summary>
        /// Executes the reset simulation.
        /// </summary>
        private void ExecuteResetSimulation()
        {
            lock (simluationLockObj)
            {
                isRunSimluation = false;
            }

            simulationTask?.Wait();

            Simulator.InitializeProtocol(this.ProtocolType);
            UpdateCurrentViewedItem();
            CheckCanExecuteCommands();
        }

        /// <summary>
        /// Determines whether this instance [can execute pause simulation].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute pause simulation]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecutePauseSimulation()
        {
            return SimulatorNetworkCreated() && isRunSimluation;
        }

        /// <summary>
        /// Determines whether this instance [can execute start simulation].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute start simulation]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteStartSimulation()
        {
            return SimulatorNetworkCreated() && !isRunSimluation;
        }

        /// <summary>
        /// Determines whether this instance [can execute reset simulation].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute reset simulation]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteResetSimulation()
        {
            return SimulatorNetworkCreated();
        }

        /// <summary>
        /// Determines whether this instance [can execute simulation step].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute simulation step]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteSimulationStep()
        {
            return SimulatorNetworkCreated() && !isRunSimluation;
        }

        /// <summary>
        /// Simulators the network created.
        /// </summary>
        /// <returns></returns>
        private bool SimulatorNetworkCreated()
        {
            return Simulator.Clients.Count > 0 && Simulator.Connections.Count > 0;
        }

        /// <summary>
        /// Adds the node.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public NetSimItem AddNode(Point location)
        {
            NetSimItem returnObj;

            lock (simluationLockObj)
            {
                returnObj = Simulator.AddClient(nextNodeName.ToString(), (int)location.X, (int)location.Y);
            }

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
                lock (simluationLockObj)
                {
                    if (!Simulator.AddConnection(from.Id, to.Id, 1))
                    {
                        draftConnection = null;
                        DrawCanvas.Children.Remove(draftConnectionLine);
                        draftConnectionLine = null;
                    }
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

            if (Mouse.DirectlyOver is System.Windows.Shapes.Path)
            {
                return (Mouse.DirectlyOver as System.Windows.Shapes.Path).Tag as NetSimItem;
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
                AddEdge(draftConnection.EndPointA as NetSimItem, draftConnection.EndPointB as NetSimItem);
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

        /// <summary>
        /// Updates the current viewed item.
        /// </summary>
        private void UpdateCurrentViewedItem()
        {
            if (CurrentViewedItem != null)
            {
                // update details view for client or connection
                this.CurrentViewedItem = CurrentViewedItem;
            }
        }

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
