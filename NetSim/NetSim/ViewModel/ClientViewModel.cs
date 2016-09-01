using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using NetSim.Lib.Simulator;
using NetSim.Lib.Simulator.Components;
using NetSim.Lib.Simulator.Messages;

namespace NetSim.ViewModel
{
    public class ClientViewModel : ViewModelBase
    {
        /// <summary>
        /// The send message command
        /// </summary>
        private ICommand sendMessageCommand;

        /// <summary>
        /// The delete clien command
        /// </summary>
        private ICommand deleteClientCommand;

        /// <summary>
        /// The send message destination
        /// </summary>
        private string sendMessageDestination;

        /// <summary>
        /// The send message data
        /// </summary>
        private string sendMessageData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientViewModel"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ClientViewModel(NetSimClient client)
        {
            this.Client = client;
            this.SendMessageData = "Data";
            InitializeCommands();
        }
        
        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public NetSimClient Client { get; }

        /// <summary>
        /// Occurs when the deletion of the client is requested.
        /// Note: http://bit.ly/2bRCkbI
        /// </summary>
        public event EventHandler<NetSimClient> DeleteClientEvent;

        /// <summary>
        /// Gets or sets the send message destination.
        /// </summary>
        /// <value>
        /// The send message destination.
        /// </value>
        public string SendMessageDestination
        {
            get { return sendMessageDestination; }
            set
            {
                sendMessageDestination = value;
                RaisePropertyChanged();
                (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
            get { return sendMessageData; }
            set
            {
                sendMessageData = value;
                RaisePropertyChanged();
                (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                return sendMessageCommand;
            }
            set
            {
                this.sendMessageCommand = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the delte client command.
        /// </summary>
        /// <value>
        /// The delte client command.
        /// </value>
        public ICommand DeleteClientCommand
        {
            get
            {
                return deleteClientCommand;
            }
            set
            {
                this.deleteClientCommand = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.SendMessageCommand = new RelayCommand(ExecuteSendMessage, CanExecuteSendMessage);
            this.DeleteClientCommand = new RelayCommand(OnDeleteClient);
        }

        /// <summary>
        /// Executes the send message.
        /// </summary>
        private void ExecuteSendMessage()
        {
            this.Client.SendMessage(new DataMessage()
            {
                Receiver = this.SendMessageDestination,
                Data = this.SendMessageData,
                Sender = this.Client.Id
            });
        }

        /// <summary>
        /// Determines whether this instance can execute send message command.
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSendMessage()
        {
            return (!string.IsNullOrEmpty(this.SendMessageDestination) && !string.IsNullOrEmpty(this.SendMessageData));
        }

        /// <summary>
        /// Called when delete client event.
        /// </summary>
        protected virtual void OnDeleteClient()
        {
            DeleteClientEvent?.Invoke(this, this.Client);
        }
    }
}
