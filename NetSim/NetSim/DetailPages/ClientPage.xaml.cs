﻿using System;
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
    /// Interaction logic for ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        /// <summary>
        /// The client
        /// </summary>
        private NetSimClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientPage"/> class.
        /// </summary>
        public ClientPage()
        {
            InitializeComponent();

        }

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
