using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NetSim.ViewModel;

namespace NetSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(DrawCanvas);
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        private MainViewModel ViewModel => this.DataContext as MainViewModel;

        /// <summary>
        /// Draws the canvas mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DrawCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.HandleMouseLeftButtonUp(sender, e);
        }

        /// <summary>
        /// Draws the canvas mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void DrawCanvasMouseMove(object sender, MouseEventArgs e)
        {
            ViewModel.HandleMouseMove(sender, e);
        }

        /// <summary>
        /// Draws the canvas mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DrawCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.HandleMouseLeftButtonDown(sender, e);
        }

    }
}
