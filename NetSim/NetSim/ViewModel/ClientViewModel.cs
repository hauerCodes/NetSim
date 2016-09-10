// -----------------------------------------------------------------------
// <copyright file="ClientViewModel.cs" company="FH Wr.Neustadt">
//      Copyright Christoph Hauer. All rights reserved.
// </copyright>
// <author>Christoph Hauer</author>
// <summary>NetSim - ClientViewModel.cs</summary>
// -----------------------------------------------------------------------

namespace NetSim.ViewModel
{
    using System;
    using System.Linq;
    using System.Windows.Input;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using NetSim.Lib.Simulator.Components;
    using NetSim.Lib.Simulator.Messages;

    /// <summary>
    /// The client view model.
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class ClientViewModel : ViewModelBase
    {
        /// <summary>
        /// The delete client command
        /// </summary>
        private ICommand deleteClientCommand;

        /// <summary>
        /// The send message command
        /// </summary>
        private ICommand sendMessageCommand;

        /// <summary>
        /// The send message data
        /// </summary>
        private string sendMessageData;

        /// <summary>
        /// The send message destination
        /// </summary>
        private string sendMessageDestination;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientViewModel" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ClientViewModel(NetSimClient client)
        {
            this.Client = client;
            this.SendMessageData = "Data";
            this.InitializeCommands();
        }

        /// <summary>
        /// Occurs when the deletion of the client is requested.
        /// </summary>
        public event EventHandler<NetSimClient> DeleteClientEvent;

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public NetSimClient Client { get; }

        /// <summary>
        /// Gets or sets the delete client command.
        /// </summary>
        /// <value>
        /// The delete client command.
        /// </value>
        public ICommand DeleteClientCommand
        {
            get
            {
                return this.deleteClientCommand;
            }

            set
            {
                this.deleteClientCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the start simulation command.
        /// </summary>
        /// <value>
        /// The start simulation command.
        /// </value>
        public ICommand SendMessageCommand
        {
            get
            {
                return this.sendMessageCommand;
            }

            set
            {
                this.sendMessageCommand = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the send message data.
        /// </summary>
        /// <value>
        /// The send message data.
        /// </value>
        public string SendMessageData
        {
            get
            {
                return this.sendMessageData;
            }

            set
            {
                this.sendMessageData = value;
                this.RaisePropertyChanged();
                (this.SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the send message destination.
        /// </summary>
        /// <value>
        /// The send message destination.
        /// </value>
        public string SendMessageDestination
        {
            get
            {
                return this.sendMessageDestination;
            }

            set
            {
                this.sendMessageDestination = value;
                this.RaisePropertyChanged();
                (this.SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Called when delete client event.
        /// </summary>
        protected virtual void OnDeleteClient()
        {
            this.DeleteClientEvent?.Invoke(this, this.Client);
        }

        /// <summary>
        /// Determines whether this instance can execute send message command.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute send message]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteSendMessage()
        {
            return !string.IsNullOrEmpty(this.SendMessageDestination) && !string.IsNullOrEmpty(this.SendMessageData);
        }

        /// <summary>
        /// Executes the send message.
        /// </summary>
        private void ExecuteSendMessage()
        {
            this.Client.SendMessage(
                new DataMessage()
                {
                    Receiver = this.SendMessageDestination,
                    Data = this.SendMessageData,
                    Sender = this.Client.Id
                });
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.SendMessageCommand = new RelayCommand(this.ExecuteSendMessage, this.CanExecuteSendMessage);
            this.DeleteClientCommand = new RelayCommand(this.OnDeleteClient);
        }
    }
}