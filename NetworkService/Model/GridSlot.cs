using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class GridSlot : BindableBase
    {
        private DistributedEnergyResource _resource = null;
        private bool _isSelected = false;
        private bool _isKeyboardFocused = false;

        #region Properties
        public DistributedEnergyResource Resource
        {
            get => _resource;
            set => SetProperty(ref _resource, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        public bool IsKeyboardFocused
        {
            get => _isKeyboardFocused;
            set => SetProperty(ref _isKeyboardFocused, value);
        }
        #endregion
        public GridSlot(){}

        public GridSlot(DistributedEnergyResource resource, bool isSelected)
        {
            Resource = resource;
            IsSelected = isSelected;
        }
    }
}
