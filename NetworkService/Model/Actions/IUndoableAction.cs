using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public interface IUndoableAction
    {
        bool Do();
        void Undo();
    }
}
