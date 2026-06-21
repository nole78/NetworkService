using System;
using System.Collections.Generic;
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

        public RemoveFromGridAction(int idx, DistributedEnergyResource[] slots)
        {
            _idx = idx;
            _slots = slots;
        }

        public bool Do()
        {
            if (_idx >= _slots.Length) return false;

            _removedResource = _slots[_idx];
            _slots[_idx] = null;

            return true;
        }

        public void Undo()
        {
            _slots[_idx] = _removedResource;
        }
    }
}
