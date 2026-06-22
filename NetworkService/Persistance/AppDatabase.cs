using NetworkService.Model;
using NetworkService.Model.Actions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Persistance
{
    public class AppDatabase : BindableBase
    {
        #region Singleton Implementation
        private static AppDatabase _instance;
        public static AppDatabase Instance => _instance ?? (_instance = new AppDatabase());
        #endregion

        private IUndoableAction _lastAction;
        public DistributedEnergyResource[] GridSlots { get; private set; } = new DistributedEnergyResource[12];
        public ObservableCollection<DistributedEnergyResource> Resources { get; private set; }
        public ObservableCollection<LineConnection> Connections { get; private set; }
        public IReadOnlyList<EnergyResourceType> ResourceTypes { get; private set; }
        public IUndoableAction LastAction
        {
            get => _lastAction;
            private set => SetProperty(ref _lastAction, value);
        }
    
        private AppDatabase()
        {
            Resources = new ObservableCollection<DistributedEnergyResource>();
            Connections = new ObservableCollection<LineConnection>();

            ResourceTypes = new List<EnergyResourceType>()
            {
                new EnergyResourceType("Solar panel", "Assets/solar panel.png"),
                new EnergyResourceType("Wind generator", "Assets/wind generator.png")
            };

            Resources.Add(new DistributedEnergyResource(1, "Solar-North", ResourceTypes[0], 3.4));
            Resources.Add(new DistributedEnergyResource(2, "Wind-West", ResourceTypes[1], 4.1));
            Resources.Add(new DistributedEnergyResource(3, "Solar-South", ResourceTypes[0], 2.8));
        }

        public bool Undo()
        {
            if (LastAction != null)
            {
                LastAction.Undo();
                LastAction = null;

                if(!(LastAction is AddResourceAction) && !(LastAction is ConnectResourceAction))
                    OnPropertyChanged(nameof(GridSlots));

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddResource(DistributedEnergyResource resource)
        {
            var addAction = new AddResourceAction(resource, Resources);
            if(addAction.Do())
            {
                LastAction = addAction;
                return true;
            }
            return false;
        }

        public bool RemoveResource(int id)
        {
            var resource = Resources.FirstOrDefault(r => r.Id == id);
            if(resource == null)
                return false;

            int idx = Resources.IndexOf(resource);

            var removeAction = new RemoveResourceAction(resource, idx, Resources, GridSlots);

            if(removeAction.Do())
            {
                LastAction = removeAction;
                return true;
            }
            return false;
        }

        public bool SetValue(int id, double value)
        {
            var resource = Resources.FirstOrDefault(r => r.Id == id);
            if (resource == null)
            {
                return false;
            }

            bool alarm = false;
            if(value < 1 || value > 5)
            {
                alarm = true;
            }
            else
            {
                alarm = false;
            }

            resource.Value = value;
            resource.IsAlarm = alarm;

            var gridResource = GridSlots.FirstOrDefault(r => r != null && r.Id == id);
            if (gridResource != null)
            {
                gridResource.Value = value;
                gridResource.IsAlarm = alarm;
            }
            return true;
        }

        public bool PlaceResourceOnGrid(DistributedEnergyResource resource, int slotIdx)
        {
            var placeAction = new PlaceResourceAction(resource, GridSlots, slotIdx);
            if (placeAction.Do())
            {
                LastAction = placeAction;

                OnPropertyChanged(nameof(GridSlots));
                return true;
            }
            return false;
        }

        public bool MoveResourceOnGrid(int fromSlotIdx, int toSlotIdx)
        {
            var moveAction = new MoveResourceAction(fromSlotIdx, toSlotIdx, GridSlots, Connections);
            if (moveAction.Do())
            {
                LastAction = moveAction;

                OnPropertyChanged(nameof(GridSlots));
                return true;
            }
            return false;
        }

        public bool RemoveResourceFromGrid(int idx)
        {
            var removeFromGridAction = new RemoveFromGridAction(idx, GridSlots, Connections);

            if(removeFromGridAction.Do())
            {
                LastAction = removeFromGridAction;
                OnPropertyChanged(nameof(GridSlots));
                return true;
            }
            return false;
        }

        public bool ConnectResourcesOnGrid(int firstSlotIdx, int secondSlotIdx)
        {
            var connectAaction = new ConnectResourceAction(firstSlotIdx, secondSlotIdx, Connections, GridSlots);

            if (connectAaction.Do())
            {
                LastAction = connectAaction;
                return true;
            }

            return false;
        }
    }
}
