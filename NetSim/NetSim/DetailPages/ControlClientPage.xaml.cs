// -----------------------------------------------------------------------
// <copyright file="ControlClientPage.xaml.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - ControlClientPage.xaml.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.DetailPages
{
    using System;
    using System.Linq;
    using System.Windows.Controls;

    using NetSim.Lib.Simulator.Components;
    using NetSim.ViewModel;

    /// <summary>
    /// Interaction logic for ControlClientPage
    /// </summary>
    public partial class ControlClientPage : Page
    {
        /// <summary>
        /// The client
        /// </summary>
        private NetSimClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlClientPage"/> class.
        /// </summary>
        public ControlClientPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when the deletion of the client is requested.
        /// </summary>
        public event EventHandler<NetSimClient> DeleteClientEvent;

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public NetSimClient Client
        {
            get
            {
                return this.client;
            }

            set
            {
                this.client = value;
                var viewModel = new ClientViewModel(this.client);

                viewModel.DeleteClientEvent += (sender, e) => this.OnDeleteClient();
                this.DataContext = viewModel;
            }
        }

        /// <summary>
        /// Called when delete client event.
        /// </summary>
        protected virtual void OnDeleteClient()
        {
            this.DeleteClientEvent?.Invoke(this, this.Client);
        }
    }
}