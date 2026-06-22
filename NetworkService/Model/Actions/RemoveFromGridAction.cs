using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class RemoveFromGridAction : IUndoableAction
    {
        private readonly int _idx;
        private readonly DistributedEnergyResource[] _slots;
        private DistributedEnergyResource _removedResource;
        private readonly ObservableCollection<LineConnection> _connections;
        private List<LineConnection> _removedConnections = new List<LineConnection>();

        public RemoveFromGridAction(int idx, DistributedEnergyResource[] slots, ObservableCollection<LineConnection> connections)
        {
            _idx = idx;
            _slots = slots;
            _connections = connections;
        }

        public bool Do()
        {
            if (_idx >= _slots.Length) return false;

            _removedConnections = _connections.Where(c => c.ToSlot ==  _idx || c.FromSlot == _idx).ToList();
            foreach (var connection in _removedConnections)
            {
                _connections.Remove(connection);
            }

            _removedResource = _slots[_idx];
            _slots[_idx] = null;

            return true;
        }

        public void Undo()
        {
            _slots[_idx] = _removedResource;
            foreach (var connection in _removedConnections)
            {
                _connections.Add(connection);
            }
        }
    }
}
