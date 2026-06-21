using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Persistance
{
    public class AppDatabase
    {
        public static DistributedEnergyResource[] GridSlots { get; set; } = new DistributedEnergyResource[12];
        public static ObservableCollection<DistributedEnergyResource> Resources { get; private set; }
        public static IReadOnlyList<EnergyResourceType> ResourceTypes { get; private set; }
    
        static AppDatabase()
        {
            Resources = new ObservableCollection<DistributedEnergyResource>();

            ResourceTypes = new List<EnergyResourceType>()
            {
                new EnergyResourceType("Solar panel", "Assets/solar panel.png"),
                new EnergyResourceType("Wind generator", "Assets/wind generator.png")
            };

            Resources.Add(new DistributedEnergyResource(1, "Solar-North", ResourceTypes[0], 3.4));
            Resources.Add(new DistributedEnergyResource(2, "Wind-West", ResourceTypes[1], 4.1));
            Resources.Add(new DistributedEnergyResource(3, "Solar-South", ResourceTypes[0], 2.8));
        }

        public static void AddResource(DistributedEnergyResource resource)
        {
            resource.Id = Resources.Count > 0 ? Resources.Max(r => r.Id) + 1 : 1;
            Resources.Add(resource);
        }

        public static bool RemoveResource(int id)
        {
            var resource = Resources.FirstOrDefault(r => r.Id == id);

            var gridResource = GridSlots.FirstOrDefault(r => r.Id == id);
            if (gridResource != null)
                gridResource = null;

            if(resource == null) return false;

            Resources.Remove(resource);
            return true;
        }

        public static bool SetValue(int id, double value)
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

            var gridResource = GridSlots.FirstOrDefault(r => r.Id == id);
            if (gridResource != null)
            {
                gridResource.Value = value;
                gridResource.IsAlarm = alarm;
            }
            return true;
        }
    }
}
