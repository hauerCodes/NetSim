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

using NetSim.Lib.Simulator;

namespace NetSim.DetailPages
{
    /// <summary>
    /// Interaction logic for ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private NetSimClient client;

        public ClientPage()
        {
            InitializeComponent();

        }

        public NetSimClient Client
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
                this.DataContext = new ClientViewModel(client);
            }
        }
    }
}
