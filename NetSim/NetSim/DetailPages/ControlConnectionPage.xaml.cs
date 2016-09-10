// -----------------------------------------------------------------------
// <copyright file="ControlConnectionPage.xaml.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - ControlConnectionPage.xaml.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.DetailPages
{
    using System;
    using System.Linq;
    using System.Windows.Controls;

    using NetSim.Lib.Simulator.Components;
    using NetSim.ViewModel;

    /// <summary>
    /// Interaction logic for ConnectionPage
    /// </summary>
    public partial class ControlConnectionPage : Page
    {
        /// <summary>
        /// The connection
        /// </summary>
        private NetSimConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlConnectionPage"/> class.
        /// </summary>
        public ControlConnectionPage()
        {
            this.InitializeComponent();
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
                return this.connection;
            }

            set
            {
                this.connection = value;
                var viewmodel = new ConnectionViewModel(this.connection);
                viewmodel.DeleteConnectionEvent += (sender, e) => this.OnDeleteConnection();

                this.DataContext = viewmodel;
            }
        }

        /// <summary>
        /// Called when the delete connection event should get fired.
        /// </summary>
        protected virtual void OnDeleteConnection()
        {
            this.DeleteConnectionEvent?.Invoke(this, this.Connection);
        }
    }
}