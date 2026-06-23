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
        private readonly GridSlot[] _slots;
        private readonly int _targetSlotIdx;
        private DistributedEnergyResource _previousResource;

        public PlaceResourceAction(DistributedEnergyResource resource, GridSlot[] slots, int targetSlotIdx)
        {
            _resource = resource;
            _slots = slots;
            _targetSlotIdx = targetSlotIdx;
        }

        public bool Do()
        {
            _previousResource = _slots[_targetSlotIdx].Resource;
            _slots[_targetSlotIdx].Resource = _resource;
            return true;
        }

        public void Undo()
        {
            if(_slots[_targetSlotIdx].Resource == null) return;

            if (_slots[_targetSlotIdx].Resource.Id != _resource.Id) return;
            
            _slots[_targetSlotIdx].Resource = _previousResource;
        }
    }
}
