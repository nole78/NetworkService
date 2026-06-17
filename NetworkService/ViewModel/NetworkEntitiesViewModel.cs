using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        private string _createResourceName;
        private EnergyResourceType _createResourceType;
        private string _searchResourceName;
        private EnergyResourceType _searchResourceType;
        private bool _searchByName = true;
        private DistributedEnergyResource _selectedResource;

        #region Properties
        public string CreateResourceName
        {
            get => _createResourceName;
            set => SetProperty(ref _createResourceName, value);
        }
        public EnergyResourceType CreateResourceType
        {
            get => _createResourceType;
            set => SetProperty(ref _createResourceType, value);
        }
        public string SearchResourceName
        {
            get => _searchResourceName;
            set => SetProperty(ref _searchResourceName, value);
        }
        public EnergyResourceType SearchResourceType
        {
            get => _searchResourceType;
            set => SetProperty(ref _searchResourceType, value);
        }
        public bool SearchByName
        {
            get => _searchByName;
            set => SetProperty(ref _searchByName, value);
        }
        public DistributedEnergyResource SelectedResource
        {
            get => _selectedResource;
            set => SetProperty(ref _selectedResource, value);
        }
        public ICollectionView EnergyResources { get; set; }
        public IReadOnlyList<EnergyResourceType> EnergyResourceTypes { get { return AppDatabase.ResourceTypes; } }
        public MyICommand CreateCommand { get; set; }
        public MyICommand DeleteCommand { get; set; }
        public MyICommand SearchCommand { get; set; }
        public MyICommand ClearSearchCommand { get; set; }
        #endregion

        public NetworkEntitiesViewModel() 
        {
            EnergyResources = CollectionViewSource.GetDefaultView(AppDatabase.Resources);
            EnergyResources.Filter = ResourceFilter;
            
            CreateCommand = new MyICommand(OnCreateCommand);
            DeleteCommand = new MyICommand(OnDeleteCommand);
            SearchCommand = new MyICommand(OnSearchCommand);
            ClearSearchCommand = new MyICommand(OnClearSearchCommand);
        }

        public bool ResourceFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchResourceName) && SearchResourceType == null)
            {
                return true;
            }

            if (item is DistributedEnergyResource resource)
            {
                if(SearchByName)
                {
                    if (string.IsNullOrEmpty(SearchResourceName)) return true;
                    return resource.Name.ToLower().Contains(SearchResourceName.ToLower());
                }
                else
                {
                    if (SearchResourceType == null) return true;
                    return resource.Type == SearchResourceType;
                }
            }
            return false;
        }

        public void OnCreateCommand()
        {
            // TODO: Add validation
            string name = CreateResourceName;
            EnergyResourceType type = CreateResourceType;

            var newResource = new DistributedEnergyResource(0, CreateResourceName, CreateResourceType, 0);
            AppDatabase.AddResource(newResource);

            CreateResourceName = "";
            CreateResourceType = null;
        }
        public void OnDeleteCommand()
        {
            if(SelectedResource != null)
            {
                AppDatabase.RemoveResource(SelectedResource.Id);
            }
        }

        public void OnSearchCommand()
        {
            EnergyResources.Refresh();
        }

        public void OnClearSearchCommand()
        {
            SearchResourceType = null;
            SearchResourceName = "";

            EnergyResources.Refresh();
        }
    }
}
