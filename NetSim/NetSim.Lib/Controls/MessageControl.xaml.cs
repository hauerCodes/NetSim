// -----------------------------------------------------------------------
// <copyright file="MessageControl.xaml.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim.Lib - MessageControl.xaml.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.Lib.Controls
{
    using System;
    using System.Linq;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for Message
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MessageControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageControl"/> class.
        /// </summary>
        public MessageControl()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageControl" /> class.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public MessageControl(string messageName)
        {
            this.InitializeComponent();

            this.MessageName.Text = messageName;
        }
    }
}