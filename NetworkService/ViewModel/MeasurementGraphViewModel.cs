using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class GraphNode : BindableBase
    {
        private double _position = 10.0;
        private double _value = 0.0;
        private bool _isHighest = false;

        #region Properties
        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }
        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public bool IsHighest
        {
            get => _isHighest;
            set => SetProperty(ref _isHighest, value);
        }
        #endregion
    }
    public class MeasurementGraphViewModel : BindableBase
    {
        private DistributedEnergyResource _selectedResource;

        #region Properties
        public DistributedEnergyResource SelectedResource
        {
            get => _selectedResource;
            set => SetProperty(ref _selectedResource, value);
        }
        public ObservableCollection<DistributedEnergyResource> Resources { get => AppDatabase.Resources; }
        public GraphNode[] Nodes { get; }
        #endregion
        public MeasurementGraphViewModel()
        {
            Nodes = new GraphNode[4] {
                Nodes[0] = new GraphNode(),
                Nodes[1] = new GraphNode(),
                Nodes[2] = new GraphNode(),
                Nodes[3] = new GraphNode()
            };
        }
    }
}
