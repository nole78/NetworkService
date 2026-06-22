using NetworkService.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class RemoveResourceAction : IUndoableAction
    {
        private readonly DistributedEnergyResource _deleteResource;
        private readonly int _resourceIdx;
        private readonly ObservableCollection<DistributedEnergyResource> _collection;
        private readonly DistributedEnergyResource[] _slots;
        private readonly ObservableCollection<LineConnection> _connections;
        private int _gridSlotIdx = -1;
        private List<LineConnection> _removedConnections = new List<LineConnection>();

        public RemoveResourceAction(DistributedEnergyResource deleteResource, int resourceIdx ,ObservableCollection<DistributedEnergyResource> collection, DistributedEnergyResource[] slots, ObservableCollection<LineConnection> connections)
        {
            _deleteResource = deleteResource;
            _resourceIdx = resourceIdx;
            _collection = collection;
            _slots = slots;
            _connections = connections;
        }
        public bool Do()
        {
            var resource = _collection.FirstOrDefault(r => r.Id == _deleteResource.Id);
            if (resource == null)
                return false;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null && _slots[i].Id == _deleteResource.Id)
                {
                    _gridSlotIdx = i;
                    _slots[i] = null; 
                    break;
                }
            }

            if (_gridSlotIdx != -1)
            {
                _removedConnections = _connections.Where(c => c.ToSlot == _gridSlotIdx || c.FromSlot == _gridSlotIdx).ToList();

                foreach(var connection in _removedConnections)
                {
                    _connections.Remove(connection);
                }
            }

            _collection.Remove(resource);
            return true;
        }

        public void Undo()
        {
            if(_collection.Count  > _resourceIdx)
            {
                _collection.Insert(_resourceIdx, _deleteResource);
            }
            else
            {
                _collection.Add(_deleteResource);
            }

            if (_gridSlotIdx != -1)
            {
                _slots[_gridSlotIdx] = _deleteResource;
                foreach (var connection in _removedConnections)
                {
                    _connections.Add(connection);
                }
            }
        }
    }
}
