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
        private const int NODE_OFFSET = 400;
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
        public string[] Times { get; }
        #endregion
        public MeasurementGraphViewModel()
        {
            Nodes = new GraphNode[4] {
                Nodes[0] = new GraphNode(),
                Nodes[1] = new GraphNode(),
                Nodes[2] = new GraphNode(),
                Nodes[3] = new GraphNode()
            };
            Times = new string[4] { "", "", "", "" };

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
                Nodes[0].Value = value;
                Times[0] = measurements[i].Time;
                if(max < value)
                    max = value;
            }

            Nodes.FirstOrDefault(n => n.Value == max).IsHighest = true;

            for(int i = 0; i < 4; i++)
            {
                var ratio = (max == 0 || Nodes[i].Value == 0)? 0 : max/Nodes[i].Value;
                Nodes[i].Position = ratio * NODE_OFFSET;
            }
        }
    }
}
