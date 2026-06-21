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

        public AddResourceAction(DistributedEnergyResource addResource, ObservableCollection<DistributedEnergyResource> collection)
        {
            _addResource = addResource;
            _collection = collection;
        }

        public bool Do()
        {
            _addResource.Id = _collection.Count > 0 ? _collection.Max(r => r.Id) + 1 : 1;
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
