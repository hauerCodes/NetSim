// -----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - MainWindow.xaml.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using NetSim.ViewModel;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainViewModel(this.DrawCanvas);
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        private MainViewModel ViewModel => this.DataContext as MainViewModel;

        /// <summary>
        /// Draws the canvas mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DrawCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.HandleMouseLeftButtonDown(sender, e);
        }

        /// <summary>
        /// Draws the canvas mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DrawCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.HandleMouseLeftButtonUp(sender, e);
        }

        /// <summary>
        /// Draws the canvas mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void DrawCanvasMouseMove(object sender, MouseEventArgs e)
        {
            this.ViewModel.HandleMouseMove(sender, e);
        }
    }
}