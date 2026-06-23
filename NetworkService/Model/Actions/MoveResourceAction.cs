using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class MoveResourceAction : IUndoableAction
    {
        private readonly int _fromSlotIdx;
        private readonly int _toSlotIdx;
        private readonly GridSlot[] _slots;
        private readonly ObservableCollection<LineConnection> _connections;

        public MoveResourceAction(int fromSlotIdx, int toSlotIdx, GridSlot[] slots, ObservableCollection<LineConnection> connections)
        {
            _fromSlotIdx = fromSlotIdx;
            _toSlotIdx = toSlotIdx;
            _slots = slots;
            _connections = connections;
        }

        public bool Do()
        {
            var len = _slots.Length;
            if (len < _fromSlotIdx || len < _toSlotIdx) return false;

            DistributedEnergyResource helper = null;
            if (_slots[_toSlotIdx].Resource != null)
            {
                helper = _slots[_toSlotIdx].Resource;
            }

            var connections = _connections.Where(c => c.ToSlot == _fromSlotIdx || c.FromSlot == _fromSlotIdx).ToList();

            foreach (var item in connections)
            {
                if (item.FromSlot == _fromSlotIdx)
                {
                    item.FromSlot = _toSlotIdx;
                }
                else if (item.ToSlot == _fromSlotIdx)
                {
                    item.ToSlot = _toSlotIdx;
                }
            }

            if(helper != null)
            {
                var connectionsMoved = _connections.Where(c => c.ToSlot == _toSlotIdx || c.FromSlot == _toSlotIdx).ToList();

                foreach (var item in connectionsMoved)
                {
                    if (item.FromSlot == _toSlotIdx)
                    {
                        item.FromSlot = _fromSlotIdx;
                    }
                    else if (item.ToSlot == _toSlotIdx)
                    {
                        item.ToSlot = _fromSlotIdx;
                    }
                }
            }

            _slots[_toSlotIdx].Resource = _slots[_fromSlotIdx].Resource;
            _slots[_fromSlotIdx].Resource = helper;

            return true;
        }

        public void Undo()
        {
            DistributedEnergyResource helper = null;
            if( _slots[_fromSlotIdx].Resource != null )
            {
                helper = _slots[_fromSlotIdx].Resource;
            }

            var connections = _connections.Where(c => c.ToSlot == _toSlotIdx || c.FromSlot == _toSlotIdx).ToList(); 
            
            foreach (var item in connections)
            {
                if (item.FromSlot == _toSlotIdx)
                {
                    item.FromSlot = _fromSlotIdx;
                }
                else if (item.ToSlot == _toSlotIdx)
                {
                    item.ToSlot = _fromSlotIdx;
                }
            }

            if (helper != null)
            {
                var connectionsMoved = _connections.Where(c => c.ToSlot == _fromSlotIdx || c.FromSlot == _fromSlotIdx).ToList();

                foreach (var item in connectionsMoved)
                {
                    if (item.FromSlot == _fromSlotIdx)
                    {
                        item.FromSlot = _toSlotIdx;
                    }
                    else if (item.ToSlot == _fromSlotIdx)
                    {
                        item.ToSlot = _toSlotIdx;
                    }
                }
            }

            _slots[_fromSlotIdx].Resource = _slots[_toSlotIdx].Resource;
            _slots[_toSlotIdx].Resource = helper;
        }
    }
}
