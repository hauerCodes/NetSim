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
using NetSim.ViewModel;

namespace NetSim.DetailPages
{
    /// <summary>
    /// Interaction logic for ConnectionPage.xaml
    /// </summary>
    public partial class ConnectionPage : Page
    {
        /// <summary>
        /// The connection
        /// </summary>
        private NetSimConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPage"/> class.
        /// </summary>
        public ConnectionPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public NetSimConnection Connection
        {
            get
            {
                return connection;
            }
            set
            {
                connection = value;
                this.DataContext = new ConnectionViewModel(connection);
            }
        }
    }
}
