using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class MoveResourceAction : IUndoableAction
    {
        private readonly int _fromSlotIdx;
        private readonly int _toSlotIdx;
        private readonly DistributedEnergyResource[] _slots;

        public MoveResourceAction(int fromSlotIdx, int toSlotIdx, DistributedEnergyResource[] slots)
        {
            _fromSlotIdx = fromSlotIdx;
            _toSlotIdx = toSlotIdx;
            _slots = slots;
        }

        public bool Do()
        {
            var len = _slots.Length;
            if (len < _fromSlotIdx || len < _toSlotIdx) return false;

            DistributedEnergyResource helper = null;
            if (_slots[_toSlotIdx] != null)
            {
                helper = _slots[_toSlotIdx];
            }

            _slots[_toSlotIdx] = _slots[_fromSlotIdx];
            _slots[_fromSlotIdx] = helper;
            return true;
        }

        public void Undo()
        {
            DistributedEnergyResource helper = null;
            if( _slots[_fromSlotIdx] != null )
            {
                helper = _slots[_fromSlotIdx];
            }

            _slots[_fromSlotIdx] = _slots[_toSlotIdx];
            _slots[_toSlotIdx] = helper;
        }
    }
}
