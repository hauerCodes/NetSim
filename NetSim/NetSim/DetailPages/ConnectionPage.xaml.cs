// -----------------------------------------------------------------------
// <copyright file="ConnectionPage.xaml.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - ConnectionPage.xaml.cs</summary>
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
            this.InitializeComponent();
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
                return this.connection;
            }

            set
            {
                this.connection = value;
                this.DataContext = new ConnectionViewModel(this.connection);
            }
        }
    }
}