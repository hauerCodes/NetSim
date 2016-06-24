﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using NetSim.Lib.Visualization;
// ReSharper disable ExplicitCallerInfoArgument

namespace NetSim.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private char nextNodeName = 'A';

        private ViewMode viewMode;

        private bool isRunSimluation;

        private object simluationLockObj = new object();

        private NetSimProtocolType protocolType;

        private NetSimConnection draftConnection;

        private Line draftConnectionLine;

        private NetSimItem currentSelectedNode;

        private NetSimItem currentViewedItem;

        private ICommand saveNetworkCommand;

        private ICommand loadNetworkCommand;

        private ICommand startSimulationCommand;

        private ICommand pauseSimulationCommand;

        private ICommand performStepCommand;

        private ICommand resetSimulationCommand;

        /// <summary>
        /// The simulation task
        /// </summary>
        private Task simulationTask;

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

                var client = value as NetSimClient;
                if (client != null)
                {
                    this.Visualizer.CurrentSelectedItem = client;
                }

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

        #endregion

        #region Initialize

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
        }

        #endregion

        #region Command Execution

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

            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show(openFileDialog.FileName);
                //todo
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

            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show(openFileDialog.FileName);
                //todo
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
            if (!Simulator.IsInitialized)
            {
                Simulator.InitializeProtocol(ProtocolType);
            }

            Simulator.PerformSimulationStep();

            UpdateCurrentViewedItem();

            CheckCanExecuteCommands();
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
                    Thread.Sleep(TimeSpan.FromSeconds(1.1));
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

        #endregion

        #region Command CanExecute Check

        private bool CanExecutePauseSimulation()
        {
            return SimulatorNetworkCreated() && isRunSimluation;
        }

        private bool CanExecuteStartSimulation()
        {
            return SimulatorNetworkCreated() && !isRunSimluation;
        }

        private bool CanExecuteResetSimulation()
        {
            return SimulatorNetworkCreated();
        }

        private bool CanExecuteSimulationStep()
        {
            return SimulatorNetworkCreated() && !isRunSimluation;
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

        #endregion

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
