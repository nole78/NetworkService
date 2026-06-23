using NetworkService.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model.Actions
{
    public class AddResourceAction : IUndoableAction
    {
        private readonly DistributedEnergyResource _addResource;
        private readonly ObservableCollection<DistributedEnergyResource> _collection;
        private static int _lastIndex = -1;

        public AddResourceAction(DistributedEnergyResource addResource, ObservableCollection<DistributedEnergyResource> collection)
        {
            _addResource = addResource;
            _collection = collection;
        }

        public bool Do()
        {
            if(_lastIndex == -1)
                _lastIndex = _collection.Count > 0 ? _collection.Max(r => r.Id) : 0;
            _addResource.Id = ++_lastIndex;
            _collection.Add(_addResource);
            return true;
        }

        public void Undo()
        {
            var resource = _collection.FirstOrDefault(r => r.Id == _addResource.Id);
            if (resource != null)
            {
                _collection.Remove(resource);
            }
        }

    }
}
