using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace NetworkService.Services
{
    public class TerminalService
    {
        private readonly List<string> _validCommands;

        public TerminalService() 
        {
            _validCommands = new List<string>()
            {
                "help",
                "add",
                "delete",
                "search",
                "nav",
                "undo"
            };
        }

        public TerminalLine HandleCommand(string line)
        {
            var arguments = line.Split(' ');
            var command = arguments[0].ToLower();
            var parameters = arguments.Skip(1).ToArray();

            if (!_validCommands.Contains(command) && !command.Equals(string.Empty))
            {
                return new TerminalLine($"There is no command matching \"{command}\" \nWrite help to get a list of all valid commands", LineType.Error);
            }

            TerminalLine response = null;
            switch(command)
            {
                case "help": response = HelpCommand();
                    break;
                case "delete": response = new TerminalLine("Not implemented yet!", LineType.Error);
                    break;
                case "add": response = new TerminalLine("Not implemented yet!", LineType.Error);
                    break;
                case "search": response = new TerminalLine("Not implemented yet!", LineType.Error);
                    break;
                case "nav": response = new TerminalLine("Not implemented yet!", LineType.Error);
                    break;
                case "undo":
                    response = new TerminalLine("Not implemented yet!", LineType.Error);
                    break;
            }
            ;

            return response;
        }

        private TerminalLine HelpCommand()
        {
            string response = "List of valid commands:\n";
            for (int i = 0; i < _validCommands.Count; i++)
            {
                response += $"\t{i + 1}." + _validCommands[i];
                if (i < _validCommands.Count - 1)
                    response += "\n";
            }
            return new TerminalLine(response, LineType.Response);
        }
    }
}
