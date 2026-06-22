using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class ConnectResourceAction : IUndoableAction
    {
        private readonly int _firstSlotIdx;
        private readonly int _secondSlotIdx;
        private readonly ObservableCollection<LineConnection> _connections;
        private readonly DistributedEnergyResource[] _slots;

        public ConnectResourceAction(int firstSlotIdx, int secondSlotIdx, ObservableCollection<LineConnection> connections, DistributedEnergyResource[] slots)
        {
            _firstSlotIdx = firstSlotIdx;
            _secondSlotIdx = secondSlotIdx;
            _connections = connections;
            _slots = slots;
        }

        public bool Do()
        {
            if (_slots.Length < _firstSlotIdx || _slots.Length < _secondSlotIdx)
                return false;

            if (_slots[_firstSlotIdx] == null || _slots[_secondSlotIdx] == null)
                return false;

            _connections.Add(new LineConnection(_firstSlotIdx, _secondSlotIdx));
            return true;
        }

        public void Undo()
        {
            var toDelete = _connections.FirstOrDefault(c => c.ToSlot == _secondSlotIdx && c.FromSlot == _firstSlotIdx);
            if (toDelete == null) 
                return;

            _connections.Remove(toDelete);
        }
    }
}
