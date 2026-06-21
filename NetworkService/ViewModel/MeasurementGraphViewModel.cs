using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Persistance;
using NetworkService.Services;
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
        private bool _isValid = false;
        private bool _isVisible = false;

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
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        #endregion
    }
    public class MeasurementGraphViewModel : BindableBase
    {
        private const int NODE_OFFSET = 265;
        private DistributedEnergyResource _selectedResource;
        private readonly MeasurmentsReader _reader;

        #region Properties
        public DistributedEnergyResource SelectedResource
        {
            get => _selectedResource;
            set 
            {
                SetProperty(ref _selectedResource, value);
                DrawGraph();
            }
        }
        public ObservableCollection<DistributedEnergyResource> Resources { get => AppDatabase.Resources; }
        public GraphNode[] Nodes { get; }
        public ObservableCollection<string> Times { get; }
        #endregion
        public MeasurementGraphViewModel()
        {
            Nodes = new GraphNode[4] {new GraphNode(),new GraphNode(),new GraphNode(), new GraphNode()};
            Times = new ObservableCollection<string>(new string[]{ "", "", "", "" });

            _reader = new MeasurmentsReader("log.txt");

            MeasurementProcessingService.OnMeasurementProcessed += OnNewMeasurment;
        }

        private void OnNewMeasurment(int idx, double value)
        {
            if (SelectedResource == null)
                return;

            int selectedIdx = AppDatabase.Resources.IndexOf(SelectedResource);
            if(selectedIdx == idx)
                DrawGraph();
        }

        private void DrawGraph()
        {
            foreach (var node in Nodes) 
            {
                node.IsVisible = false;
                node.IsValid = false;
                node.Value = 0;
            }

            if (SelectedResource == null)
                return;

            int selectedIdx = AppDatabase.Resources.IndexOf(SelectedResource);
            if (selectedIdx < 0)
                return;

            var measurements = _reader.ReadMeasurments(selectedIdx);

            double max = 0;
            for(int i = 0; i < measurements.Count; i++)
            {
                var value = measurements[i].Value;
                Nodes[i].Value = value;
                Nodes[i].IsVisible = true;

                if(value >= 1 && value <= 5)
                    Nodes[i].IsValid = true;

                Times[i] = measurements[i].Time;
                if(max < value)
                    max = value;
            }

            for(int i = 0; i < 4; i++)
            {
                var ratio = Math.Abs(Nodes[i].Value - max);
                if (ratio != 0 && max != 0)
                    ratio = ratio / max;

                Nodes[i].Position = ratio * NODE_OFFSET;
            }
        }
    }
}
