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

using NetSim.ViewModels;

namespace NetSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(DrawCanvas);
        }

        private MainViewModel ViewModel => this.DataContext as MainViewModel;

        private void DrawCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.HandleMouseLeftButtonUp(sender, e);
        }

        private void DrawCanvasMouseMove(object sender, MouseEventArgs e)
        {
            ViewModel.HandleMouseMove(sender, e);
        }

        private void DrawCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.HandleMouseLeftButtonDown(sender, e);
        }
    }
}
