using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class PlaceResourceAction : IUndoableAction
    {
        private readonly DistributedEnergyResource _resource;
        private readonly DistributedEnergyResource[] _slots;
        private readonly int _targetSlotIdx;

        public PlaceResourceAction(DistributedEnergyResource resource, DistributedEnergyResource[] slots, int targetSlotIdx)
        {
            _resource = resource;
            _slots = slots;
            _targetSlotIdx = targetSlotIdx;
        }

        public bool Do()
        {
            _slots[_targetSlotIdx] = _resource;
            return true;
        }

        public void Undo()
        {
            if(_slots[_targetSlotIdx] == null) return;

            if (_slots[_targetSlotIdx].Id == _resource.Id)
            {
                _slots[_targetSlotIdx] = null;
            }
        }
    }
}
