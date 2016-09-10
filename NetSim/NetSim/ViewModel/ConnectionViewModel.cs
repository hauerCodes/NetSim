// -----------------------------------------------------------------------
// <copyright file="ConnectionViewModel.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - ConnectionViewModel.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using NetSim.Lib.Simulator.Components;

    /// <summary>
    /// The connection view model.
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class ConnectionViewModel : ViewModelBase
    {
        /// <summary>
        /// The delete client command
        /// </summary>
        private ICommand deleteConnectionCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionViewModel"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ConnectionViewModel(NetSimConnection connection)
        {
            this.Connection = connection;
            this.InitializeCommands();
        }

        /// <summary>
        /// Occurs when the deletion of the connection is requested.
        /// </summary>
        public event EventHandler<NetSimConnection> DeleteConnectionEvent;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public NetSimConnection Connection { get; }

        /// <summary>
        /// Gets the current messages.
        /// </summary>
        /// <value>
        /// The current messages.
        /// </value>
        public List<string> CurrentMessages
        {
            get
            {
                return this.Connection.PendingMessages.Select(m => m.ToString()).ToList();

                // StringBuilder builder = new StringBuilder();
                // foreach(var message in this.Connection.PendingMessages)
                // {
                // builder.AppendLine(message.ToString());
                // }

                // return builder.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the delete client command.
        /// </summary>
        /// <value>
        /// The delete client command.
        /// </value>
        public ICommand DeleteConnectionCommand
        {
            get
            {
                return this.deleteConnectionCommand;
            }

            set
            {
                this.deleteConnectionCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Called when delete client event.
        /// </summary>
        protected virtual void OnDeleteConnection()
        {
            this.DeleteConnectionEvent?.Invoke(this, this.Connection);
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.DeleteConnectionCommand = new RelayCommand(this.OnDeleteConnection);
        }
    }
}