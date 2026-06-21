using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace NetworkService.Services
{
    public class TerminalService
    {
        private readonly List<string> _validCommands;
        private static readonly Regex AddRegex = new Regex(@"^\s*add\s+""([^""]+)""\s+""([^""]+)""\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex DeleteRegex = new Regex(@"^\s*delete\s+([1-9][0-9]*)\s*$", RegexOptions.IgnoreCase);

        public TerminalService() 
        {
            _validCommands = new List<string>()
            {
                "help",
                "undo",
                "add",
                "delete",
                "search",
                "nav"
            };
        }

        public TerminalLine HandleCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            if (!_validCommands.Contains(command))
            {
                return new TerminalLine($"There is no command matching \"{command}\" \nWrite help to get a list of all valid commands", LineType.Error);
            }

            switch(command)
            {
                case "help": 
                    return HelpCommand();
                case "undo":
                    return UndoCommand();
                case "add": 
                    return AddCommand(line);
                case "delete":
                    return DeleteCommand(line);
                case "search": 
                case "nav": 
                    return new TerminalLine("Not implemented yet!", LineType.Error);
                default:
                    return new TerminalLine("Unknown error", LineType.Error);
            }
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

        private TerminalLine AddCommand(string line)
        {
            Match match = AddRegex.Match(line);

            if(!match.Success)
            {
                return new TerminalLine("Invalid add arguments or syntax error.\n=> Format: add \"[resource name]\" \"[resource type]\"", LineType.Error);
            }

            string resourceName = match.Groups[1].Value.Trim();
            string resourceTypeName = match.Groups[2].Value.Trim();

            var type = AppDatabase.Instance.ResourceTypes.FirstOrDefault(r => r.Name.ToLower() == resourceTypeName.ToLower());
            if(type == null)
            {
                var typeList = AppDatabase.Instance.ResourceTypes.Select(r => r.Name);
                string typeNames = "\t" + string.Join("\n\t", typeList);
                return new TerminalLine($"Error: Resource type \"{resourceTypeName}\" doesn't exist!\n=> Existing types:\n" + typeNames, LineType.Error);
            }

            var newResource = new DistributedEnergyResource(0, resourceName, type, null);
            AppDatabase.Instance.AddResource(newResource);

            return new TerminalLine($"Succesfully added new resource:\nID: {newResource.Id} | Name: {resourceName} | Type: {type.Name}", LineType.Success);
        }

        private TerminalLine DeleteCommand(string line)
        {
            Match match = DeleteRegex.Match(line);

            if (!match.Success)
            {
                return new TerminalLine("Invalid delete arguments or syntax error.\n=> Format: delete [resource_id]", LineType.Error);
            }

            string param = match.Groups[1].Value.Trim();

            if (int.TryParse(param, out int id) && id > 0)
            {
                var resource = AppDatabase.Instance.Resources.FirstOrDefault(r => r.Id == id);
                if(resource == null)
                {
                    return new TerminalLine($"Error: Resource with ID: {id} doesn't exist", LineType.Error);
                }
                else
                {
                    if(AppDatabase.Instance.RemoveResource(id))
                    {
                        return new TerminalLine($"Succesfuly removed resource:\nID: {id} | Name: {resource.Name} | Type: {resource.Type.Name}", LineType.Success);
                    }
                    else
                    {
                        return new TerminalLine($"Error: Failed to remove resource with ID: {id}", LineType.Error);
                    }
                }
            }
            else
            {    
                return new TerminalLine($"Invalid delete arguments.\n=> delete {param}\n\t\t\t^\n\t\t\t└-- id must be a number grater than 0!", LineType.Error);            
            }
        }

        private TerminalLine UndoCommand()
        {
            if(AppDatabase.Instance.Undo())
            {
                return new TerminalLine($"Succesfuly undone the last action", LineType.Success);
            }
            else
            {
                return new TerminalLine($"Error: There is no action to undo!", LineType.Error);
            }

        }
    }
}
