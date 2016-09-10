// -----------------------------------------------------------------------
// <copyright file="InfoWindow.xaml.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - InfoWindow.xaml.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim
{
    using System;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for InfoWindow
    /// </summary>
    public partial class InfoWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoWindow"/> class.
        /// </summary>
        public InfoWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Buttons the click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}