using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class LineConnection : BindableBase
    {
        private int _fromSlot;
        private int _toSlot;
        public int FromSlot
        {
            get => _fromSlot;
            set => SetProperty(ref _fromSlot, value);
        }
        public int ToSlot
        {
            get => _toSlot;
            set => SetProperty(ref _toSlot, value);
        }

        public LineConnection(){ }

        public LineConnection(int fromSlot, int toSlot)
        {
            FromSlot = fromSlot;
            ToSlot = toSlot;
        }
    }
}
