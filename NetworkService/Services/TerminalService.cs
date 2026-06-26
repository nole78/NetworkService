using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NetworkService.Services
{
    public class TerminalService
    {
        private bool _isWaiting = false;
        private string _actionWaiting = string.Empty;
        private string _lineWaiting = string.Empty;
        private readonly List<string> _validCommands;
        private readonly List<string> _validGridCommands;
        // grid add "test" "solar panel"
        private static readonly Regex AddRegex = new Regex(@"^\s*add\s+""([^""]+)""\s+""([^""]+)""\s*$", RegexOptions.IgnoreCase);
        // grid delete 3
        private static readonly Regex DeleteRegex = new Regex(@"^\s*delete\s+([1-9][0-9]*)\s*$", RegexOptions.IgnoreCase);
        // grid remove 3
        private static readonly Regex GridRemoveRegex = new Regex(@"^\s*grid\s+remove\s+(\d+)\s*$", RegexOptions.IgnoreCase);
        // grid move 3 to 4
        private static readonly Regex GridMoveRegex = new Regex(@"^\s*grid\s+move\s+(\d+)\s+to\s+(\d+)\s*$", RegexOptions.IgnoreCase);
        // grid put 3 into 4
        private static readonly Regex GridPutRegex = new Regex(@"^\s*grid\s+put\s+(\d+)\s+into\s+(\d+)\s*$", RegexOptions.IgnoreCase);
        // grid connect 3 to 4
        private static readonly Regex GridConnectRegex = new Regex(@"^\s*grid\s+connect\s+(\d+)\s+to\s+(\d+)\s*$", RegexOptions.IgnoreCase);

        public TerminalService()
        {
            _validCommands = new List<string>()
            {
                "help",
                "undo",
                "add",
                "delete",
                "grid",
            };

            _validGridCommands = new List<string>()
            {
                "help",
                "move",
                "remove",
                "put",
                "connect"
            };
        }

        public TerminalLine HandleCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            if(_isWaiting)
            {
                switch(command)
                {
                    case "y":
                        return ExecuteWaitingCommand(_actionWaiting, _lineWaiting);
                    case "n":
                        {
                            ClearWaiting();
                            return new TerminalLine("Action canceled", LineType.Response);
                        }      
                    default:
                        return new TerminalLine("Error: Invalid command!\n=> Expected:\t\"Y\" -> approval of action \n\t\t\"N\" -> denial of action", LineType.Error);
                }
            }

            if (!_validCommands.Contains(command))
            {
                return new TerminalLine($"There is no command matching \"{command}\" \nWrite help to get a list of all valid commands", LineType.Error);
            }

            return ExecuteCommand(command, line);
        }

        private TerminalLine ExecuteCommand(string command, string line)
        {
            switch (command)
            {
                case "help":
                    return HelpCommand();
                case "undo":
                    return UndoCommand();
                case "add":
                    return AddCommand(line);
                case "delete":
                    return DeleteCommand(line);
                case "grid":
                    return ExecuteGridCommand(line);
                default:
                    return new TerminalLine("Unknown error", LineType.Error);
            }
        }

        private TerminalLine ExecuteWaitingCommand(string command, string line)
        {
            switch (command)
            {
                case "grid put":
                    return ExecuteGridPutCommand(line);
                case "grid move":
                    return ExecuteGridMoveCommand(line);
                case "grid connect":
                    return ExecuteGridConnectCommand(line);
                case "grid remove":
                    return ExecuteGridRemoveCommand(line);
                default:
                    return ExecuteCommand(command, line);
            }
        }

        #region Regular Commands Implementation
        private TerminalLine HelpCommand()
        {
            string response = "List of valid commands:\n";
            response += "\thelp\t- get list of all commands\n" +
                "\tundo\t- undo last command\n" +
                "\tadd\t- add new resource\n" +
                "\tdelete\t- delete existing resource\n" +
                "\tgrid\t- grid modifier commands (write \"grid help\" to get list of all grid commands)";
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

            if (_isWaiting)
            {
                ClearWaiting();

                var newResource = new DistributedEnergyResource(0, resourceName, type, null);
                AppDatabase.Instance.AddResource(newResource);

                return new TerminalLine($"Succesfully added new resource:\nID: {newResource.Id} | Name: {resourceName} | Type: {type.Name}", LineType.Success);
            }
            else
            {
                _isWaiting = true;
                _actionWaiting = "add";
                _lineWaiting = line;
                return GetApproval($"add resoruce {resourceName}");
            }
        }

        private TerminalLine DeleteCommand(string line)
        {
            Match match = DeleteRegex.Match(line);

            if (!match.Success)
            {
                return new TerminalLine("Invalid delete arguments or syntax error.\n=> Format: delete [resource_id]", LineType.Error);
            }

            string param = match.Groups[1].Value.Trim();

            if (int.TryParse(param, out int id) && id >= 0)
            {
                var resource = AppDatabase.Instance.Resources.FirstOrDefault(r => r.Id == id);
                if(resource == null)
                {
                    return new TerminalLine($"Error: Resource with ID: {id} doesn't exist", LineType.Error);
                }
                else
                {
                    if (_isWaiting)
                    {
                        ClearWaiting();
                        if (AppDatabase.Instance.RemoveResource(id))
                        {
                            return new TerminalLine($"Succesfuly removed resource:\nID: {id} | Name: {resource.Name} | Type: {resource.Type.Name}", LineType.Success);
                        }
                        else
                        {
                            return new TerminalLine($"Error: Failed to remove resource with ID: {id}", LineType.Error);
                        }
                    }
                    else
                    {
                        _isWaiting = true;
                        _actionWaiting = "delete";
                        _lineWaiting = line;
                        return GetApproval($"delete resource {resource.Name}");
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
        #endregion

        #region Grid Commands Implementation
        private TerminalLine ExecuteGridCommand(string line)
        {
            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length < 2)
            {
                return ExecuteGridHelpCommand();
            }
            var command = parts[1].ToLower();

            if (!_validGridCommands.Contains(command))
            {
                return new TerminalLine($"There is no command matching \"grid {command}\" \nWrite \"grid help\" to get a list of all valid grid commands", LineType.Error);
            }

            switch(command)
            {
                case "put": 
                    return ExecuteGridPutCommand(line);
                case "move":
                    return ExecuteGridMoveCommand(line);
                case "help":
                    return ExecuteGridHelpCommand();
                case "remove":
                    return ExecuteGridRemoveCommand(line);
                case "connect":
                    return ExecuteGridConnectCommand(line);
                default:
                    return new TerminalLine("Unknown error", LineType.Error);
            }
        }

        private TerminalLine ExecuteGridHelpCommand()
        {
            string response = "List of valid grid commands:\n";
            response += "\thelp\t- get a list of all valid grid commands\n" +
                        "\tmove\t- move resource from one grid slot to another\n" +
                        "\tremove\t- remove resource from the grid slot\n" +
                        "\tput\t- put existing resource onto the grid\n" +
                        "\tconnect\t- connect two resources on the grid";
            return new TerminalLine(response, LineType.Response);
        }

        private TerminalLine ExecuteGridPutCommand(string line)
        {
            Match match = GridPutRegex.Match(line);

            if (!match.Success)
            {
                return new TerminalLine("Invalid grid put arguments or syntax error.\n=> Format: grid put \"[resource id]\" into \"[grid slot index]\"", LineType.Error);
            }

            string resourceIdString = match.Groups[1].Value.Trim();
            string gridSlotIdxString = match.Groups[2].Value.Trim();

            if(!int.TryParse(resourceIdString, out int resoruceId) || resoruceId <= 0)
            {
                return new TerminalLine($"Invalid grid put arguments.\n=> {line}\n id must be a number grater than 0!", LineType.Error);
            }

            if(!int.TryParse(gridSlotIdxString, out int gridSlotIdx) || gridSlotIdx < 0 || gridSlotIdx > 12)
            {
                return new TerminalLine($"Invalid grid put arguments.\n=> {line} \n grid index must be a number between 0 and 11!", LineType.Error);
            }

            var resource = AppDatabase.Instance.Resources.FirstOrDefault(r => r.Id == resoruceId);
            if(resource == null)
            {
                return new TerminalLine($"Error: There is no resource with id {resoruceId}!", LineType.Error);
            }
            var gridResource = AppDatabase.Instance.GridSlots.FirstOrDefault(s => s.Resource != null && s.Resource.Id == resource.Id);
            if(gridResource != null)
            {
                return new TerminalLine($"Error: Resource \"{resource.Name}\" is already in grid!", LineType.Error);
            }

            if (_isWaiting)
            {
                ClearWaiting();
                if (AppDatabase.Instance.PlaceResourceOnGrid(resource, gridSlotIdx))
                {

                    return new TerminalLine($"Succesfully moved resource \"{resource.Name}\" into slot {gridSlotIdx}", LineType.Success);
                }
                else
                {
                    return new TerminalLine($"Error: Failed to put resoruce \"{resource.Name}\" into slot {gridSlotIdx}!", LineType.Error);
                }
            }
            else
            {
                _isWaiting = true;
                _actionWaiting = "grid put";
                _lineWaiting = line;
                return GetApproval("put resource into grid");
            }
        }

        private TerminalLine ExecuteGridMoveCommand(string line)
        {
            Match match = GridMoveRegex.Match(line);

            if (!match.Success)
            {
                return new TerminalLine("Invalid grid move arguments or syntax error.\n=> Format: grid move \"[from grid slot index]\" to \"[to grid slot index]\"", LineType.Error);
            }

            string gridFromSlotIdxString = match.Groups[1].Value.Trim();
            string gridToSlotIdxString = match.Groups[2].Value.Trim();

            if (!int.TryParse(gridFromSlotIdxString, out int gridFromSlotIdx) || gridFromSlotIdx > 12 || gridFromSlotIdx < 0)
            {
                return new TerminalLine($"Invalid grid move arguments.\n=> {line}\n grid index must be a number between 0 and 11!", LineType.Error);
            }

            if (!int.TryParse(gridToSlotIdxString, out int gridToSlotIdx) || gridFromSlotIdx > 12 || gridFromSlotIdx < 0)
            {
                return new TerminalLine($"Invalid grid move arguments.\n=> {line} \n grid index must be a number between 0 and 11!", LineType.Error);
            }

            var slotFromResource = AppDatabase.Instance.GridSlots[gridFromSlotIdx].Resource;
            if (slotFromResource == null)
            {
                return new TerminalLine($"Error: There is no resource in grid slot {gridFromSlotIdx}!", LineType.Error);
            }

            var slotToResource = AppDatabase.Instance.GridSlots[gridToSlotIdx].Resource;


            if (_isWaiting)
            {
                ClearWaiting();
                if (AppDatabase.Instance.MoveResourceOnGrid(gridFromSlotIdx, gridToSlotIdx))
                {
                    if (slotToResource == null)
                        return new TerminalLine($"Succesfully moved resource \"{slotFromResource.Name}\" to slot {gridToSlotIdx}", LineType.Success);
                    else
                        return new TerminalLine($"Succesfully switched places for resources \"{slotFromResource.Name}\" and \"{slotToResource.Name}\"", LineType.Success);
                }
                else
                {
                    if (slotToResource == null)
                        return new TerminalLine($"Error: Failed to move resoruce \"{slotFromResource.Name}\" to slot {gridToSlotIdx}!", LineType.Error);
                    else
                        return new TerminalLine($"Error: Failed to switch places for resources \"{slotFromResource.Name}\" and \"{slotToResource.Name}\"", LineType.Success);
                }
            }
            else
            {
                _isWaiting = true;
                _actionWaiting = "grid move";
                _lineWaiting = line;
                return GetApproval("move resource");
            }
        }

        private TerminalLine ExecuteGridRemoveCommand(string line)
        {
            Match match = GridRemoveRegex.Match(line);

            if (!match.Success)
            {
                return new TerminalLine("Invalid grid remove arguments or syntax error.\n=> Format: grid remove [grid slot index]", LineType.Error);
            }

            string gridSlotIdxString = match.Groups[1].Value.Trim();

            if (!int.TryParse(gridSlotIdxString, out int gridSlotIdx) || gridSlotIdx < 0 || gridSlotIdx > 12)
            {
                return new TerminalLine($"Invalid grid remove arguments.\n=> {line} \n grid slot index must be a number between 0 and 11!", LineType.Error);
            }

            var resource = AppDatabase.Instance.GridSlots[gridSlotIdx].Resource;
            if (resource == null)
            {
                return new TerminalLine($"Error: There is no resource in {gridSlotIdx + 1}. sloth!", LineType.Error);
            }

            if (_isWaiting)
            {
                ClearWaiting();
                if (AppDatabase.Instance.RemoveResourceFromGrid(gridSlotIdx))
                {

                    return new TerminalLine($"Succesfully removed resource from the grid\nID: {resource.Id} | Name: {resource.Name} | Type: {resource.Type.Name}", LineType.Success);
                }
                else
                {
                    return new TerminalLine($"Error: Failed to remove from the grid, resoruce at the {gridSlotIdx}. slot!", LineType.Error);
                }
            }
            else
            {
                _isWaiting = true;
                _actionWaiting = "grid remove";
                _lineWaiting = line;
                return GetApproval("remove resource from the grid");
            }
        }

        private TerminalLine ExecuteGridConnectCommand(string line)
        {
            Match match = GridConnectRegex.Match(line);

            if (!match.Success)
            {
                return new TerminalLine("Invalid grid connect arguments or syntax error.\n=> Format: grid connect \"[from grid slot index]\" to \"[to grid slot index]\"", LineType.Error);
            }

            string gridFromSlotIdxString = match.Groups[1].Value.Trim();
            string gridToSlotIdxString = match.Groups[2].Value.Trim();

            if (!int.TryParse(gridFromSlotIdxString, out int gridFromSlotIdx) || gridFromSlotIdx > 12 || gridFromSlotIdx < 0)
            {
                return new TerminalLine($"Invalid grid connect arguments.\n=> {line}\n grid slot index must be a number between 0 and 11!", LineType.Error);
            }

            if (!int.TryParse(gridToSlotIdxString, out int gridToSlotIdx) || gridFromSlotIdx > 12 || gridFromSlotIdx < 0)
            {
                return new TerminalLine($"Invalid grid connect arguments.\n=> {line} \n grid slot index must be a number between 0 and 11!", LineType.Error);
            }

            var slotFromResource = AppDatabase.Instance.GridSlots[gridFromSlotIdx].Resource;
            if (slotFromResource == null)
            {
                return new TerminalLine($"Error: There is no resource in grid slot {gridFromSlotIdx}!", LineType.Error);
            }

            var slotToResource = AppDatabase.Instance.GridSlots[gridToSlotIdx].Resource;
            if (slotToResource == null)
            {
                return new TerminalLine($"Error: There is no resource in grid slot {gridToSlotIdx}!", LineType.Error);
            }

            if (_isWaiting)
            {
                ClearWaiting();
                if (AppDatabase.Instance.ConnectResourcesOnGrid(gridFromSlotIdx, gridToSlotIdx))
                {
                    return new TerminalLine($"Succesfully connected resources \"{slotFromResource.Name}\" and \"{slotToResource.Name}\"", LineType.Success);
                }
                else
                {
                    return new TerminalLine($"Error: Failed to connect resources \"{slotFromResource.Name}\" and \"{slotToResource.Name}\"", LineType.Success);
                }
            }
            else
            {
                _isWaiting = true;
                _actionWaiting = "grid connect";
                _lineWaiting = line;
                return GetApproval("connect these resources");
            }
        }
        #endregion

        private TerminalLine GetApproval(string message)
        {
            return new TerminalLine($"Are you sure you want to {message}? (Y/N)", LineType.Question);
        }

        private void ClearWaiting()
        {
            _isWaiting = false;
            _actionWaiting = "";
            _lineWaiting = "";
        }

    }
}
