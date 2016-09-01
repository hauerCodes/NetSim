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

namespace NetSim.ViewModel
{
    public class ConnectionViewModel : ViewModelBase
    {

        /// <summary>
        /// The delete clien command
        /// </summary>
        private ICommand deleteConnectionCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionViewModel"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ConnectionViewModel(NetSimConnection connection)
        {
            this.Connection = connection;
            InitializeCommands();
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.DeleteConnectionCommand = new RelayCommand(OnDeleteConnection);
        }

        /// <summary>
        /// Occurs when the deletion of the connection is requested.
        /// Note: http://bit.ly/2bRCkbI - best practice consideration
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
        /// Gets or sets the delte client command.
        /// </summary>
        /// <value>
        /// The delte client command.
        /// </value>
        public ICommand DeleteConnectionCommand
        {
            get
            {
                return deleteConnectionCommand;
            }
            set
            {
                this.deleteConnectionCommand = value;
                RaisePropertyChanged();
            }
        }

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

                //StringBuilder builder = new StringBuilder();
                //foreach(var message in this.Connection.PendingMessages)
                //{
                //    builder.AppendLine(message.ToString());
                //}

                //return builder.ToString();
            }
        }

        /// <summary>
        /// Called when delete client event.
        /// </summary>
        protected virtual void OnDeleteConnection()
        {
            DeleteConnectionEvent?.Invoke(this, this.Connection);
        }
    }

}
