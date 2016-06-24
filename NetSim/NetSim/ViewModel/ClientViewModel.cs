using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using NetSim.Lib.Simulator;

namespace NetSim.ViewModel
{
    public class ClientViewModel : ViewModelBase
    {
        /// <summary>
        /// The send message command
        /// </summary>
        private ICommand sendMessageCommand;

        /// <summary>
        /// The send message destination
        /// </summary>
        private string sendMessageDestination;

        /// <summary>
        /// The send message data
        /// </summary>
        private string sendMessageData;

        #region Constructor

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public NetSimClient Client { get; }

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

        #endregion

        #region Commands

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

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.SendMessageCommand = new RelayCommand(ExecuteSendMessage, CanExecuteSendMessage);
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

        #endregion
    }
}
