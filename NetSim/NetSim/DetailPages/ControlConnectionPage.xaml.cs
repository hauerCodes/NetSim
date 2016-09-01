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
using NetSim.Lib.Simulator.Components;
using NetSim.ViewModel;

namespace NetSim.DetailPages
{
    /// <summary>
    /// Interaction logic for ConnectionPage.xaml
    /// </summary>
    public partial class ControlConnectionPage : Page
    {
        /// <summary>
        /// The connection
        /// </summary>
        private NetSimConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPage"/> class.
        /// </summary>
        public ControlConnectionPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when the deletion of the connection is requested.
        /// Note: http://bit.ly/2bRCkbI - best practice consideration
        /// </summary>
        public event EventHandler<NetSimConnection> DeleteConnectionEvent;

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
                var viewmodel = new ConnectionViewModel(connection);
                viewmodel.DeleteConnectionEvent += (sender, e) => OnDeleteConnection();

                this.DataContext = viewmodel;
            }
        }

        /// <summary>
        /// Called when the delete connection event should get fired.
        /// </summary>
        protected virtual void OnDeleteConnection()
        {
            DeleteConnectionEvent?.Invoke(this, this.Connection);
        }
    }
}
