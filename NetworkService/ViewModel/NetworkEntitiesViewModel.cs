using NetworkService.Helpers;
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
        private DistributedEnergyResource _createEnergyResource = new DistributedEnergyResource();
        private string _searchResourceName;
        private EnergyResourceType _searchResourceType;
        private bool _searchByName = true;
        private DistributedEnergyResource _selectedResource;

        #region Properties
        public DistributedEnergyResource CreateEnergyResource
        {
            get => _createEnergyResource;
            set => SetProperty(ref _createEnergyResource, value);
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
        public IReadOnlyList<EnergyResourceType> EnergyResourceTypes { get { return AppDatabase.Instance.ResourceTypes; } }
        public MyICommand CreateCommand { get; set; }
        public MyICommand DeleteCommand { get; set; }
        public MyICommand SearchCommand { get; set; }
        public MyICommand ClearSearchCommand { get; set; }
        #endregion

        public NetworkEntitiesViewModel() 
        {
            EnergyResources = CollectionViewSource.GetDefaultView(AppDatabase.Instance.Resources);
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
            CreateEnergyResource.Validate();
            if(CreateEnergyResource.IsValid)
            {
                DistributedEnergyResource newResource = new DistributedEnergyResource(0, CreateEnergyResource.Name, CreateEnergyResource.Type, null);
                AppDatabase.Instance.AddResource(newResource);


                CreateEnergyResource.Name = "";
                CreateEnergyResource.Type = null;
            }
        }
        public void OnDeleteCommand()
        {
            if(SelectedResource != null)
            {
                AppDatabase.Instance.RemoveResource(SelectedResource.Id);
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
