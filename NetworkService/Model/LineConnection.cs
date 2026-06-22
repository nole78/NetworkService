using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class LineConnection
    {
        public int FromSlot { get; set; }
        public int ToSlot { get; set; }

        public LineConnection(){ }

        public LineConnection(int fromSlot, int toSlot)
        {
            FromSlot = fromSlot;
            ToSlot = toSlot;
        }
    }
}
