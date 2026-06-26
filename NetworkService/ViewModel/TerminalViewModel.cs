using NetworkService.Model;
using NetworkService.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class TerminalViewModel : BindableBase
    {
        private string _command;
        private const string COMMAND_BEGINING = "renewable_scada_terminal> ";
        private readonly TerminalService _terminalService;
        private readonly List<string> _terminalCommandsHistory = new List<string>();
        private int _historyIndex = -1;
        private string _savedInput = "";
        private TerminalLine _newestLine;

        #region Properties
        public string Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }
        public TerminalLine NewestLine
        {
            get => _newestLine;
            set => SetProperty(ref _newestLine, value);
        }
        public MyICommand ExecuteTerminalCommand { get; set; }
        public MyICommand GetOlderCommand { get; set; }
        public MyICommand GetNewerCommand { get; set; }
        public ObservableCollection<TerminalLine> TerminalLines { get; set; } = new ObservableCollection<TerminalLine>();
        #endregion

        public TerminalViewModel()
        {
            ExecuteTerminalCommand = new MyICommand(OnTerminalCommandExecute);
            GetOlderCommand = new MyICommand(OnGetOlderCommand);
            GetNewerCommand = new MyICommand(OnGetNewerCommand);
            _terminalService = new TerminalService();
            Command = "";
            TerminalLines.Add(new TerminalLine("Write help to get a list of all valid commands", LineType.Response));
        }

        private void OnTerminalCommandExecute()
        {
            var commandLine = new TerminalLine(COMMAND_BEGINING + Command, LineType.Command);
            TerminalLines.Add(commandLine);

            if (!Command.Equals(string.Empty))
            {
                _terminalCommandsHistory.Add(Command);
            }

            var response = _terminalService.HandleCommand(Command);
            if (response != null)
            {
                TerminalLines.Add(response);
            }

            Command = "";
            _historyIndex = -1;
            _savedInput = "";
            NewestLine = TerminalLines.Last();
        }

        private void OnGetOlderCommand()
        {
            int cnt = _terminalCommandsHistory.Count() - 1;

            if (_historyIndex == cnt)
            {
                return;
            }

            _historyIndex++;

            if(_historyIndex == 0)
            {
                _savedInput = Command;
            }

            Command = _terminalCommandsHistory[cnt - _historyIndex];
        }

        private void OnGetNewerCommand()
        {
            if (_historyIndex == -1)
            {
                return;
            }

            _historyIndex--;
            if (_historyIndex == -1)
            {
                Command = _savedInput;
                return;
            }
            
            int cnt = _terminalCommandsHistory.Count() - 1;
            if (cnt > 0)
            {
                Command = _terminalCommandsHistory[cnt - _historyIndex];
            }
        }
    }
}
