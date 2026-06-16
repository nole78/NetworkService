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
        public static ObservableCollection<DistributedEnergyResource> Resources { get; private set; }
        public static IReadOnlyList<EnergyResourceType> ResourceTypes { get; private set; }
    
        static AppDatabase()
        {
            Resources = new ObservableCollection<DistributedEnergyResource>();

            ResourceTypes = new List<EnergyResourceType>()
            {
                new EnergyResourceType("Solar panel", "solar.png"),
                new EnergyResourceType("Wind generator", "wind.png")
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
                
            resource.Value = value;
            return true;
        }
    }
}
