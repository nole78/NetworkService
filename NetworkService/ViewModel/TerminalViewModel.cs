using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class TerminalViewModel : BindableBase
    {
        private string _command;
        private const string COMMAND_BEGINING = "renewable_scada_terminal> ";

        #region Properties
        public string Command
        {
            get => _command;
            set 
            {
                if (_command != value)
                {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }
        public MyICommand TerminalCommand { get; set; }
        public List<TerminalLine> TerminalCommandsHistory { get; set; } = new List<TerminalLine>();
        public ObservableCollection<TerminalLine> TerminalLines { get; set; } = new ObservableCollection<TerminalLine>();
        #endregion

        public TerminalViewModel()
        {
            TerminalCommand = new MyICommand(OnTerminalCommand);
        }

        private void OnTerminalCommand()
        {
            var commandLine = new TerminalLine(COMMAND_BEGINING + Command, LineType.Command);
            TerminalLines.Add(commandLine);
            TerminalCommandsHistory.Add(commandLine);
            TerminalLines.Add(new TerminalLine("Command received", LineType.Success));
            Command = "";
        }
    }
}
