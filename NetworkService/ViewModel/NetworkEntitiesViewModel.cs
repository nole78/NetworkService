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
        private bool _filterActive = false;
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
        public bool FilterActive
        {
            get => _filterActive;
            set
            {
                SetProperty(ref _filterActive, value);
                ClearSearchCommand.RaiseCanExecuteChanged();
            }

        }
        public DistributedEnergyResource SelectedResource
        {
            get => _selectedResource;
            set => SetProperty(ref _selectedResource, value);
        }
        public ICollectionView EnergyResources { get; set; }
        public IReadOnlyList<EnergyResourceType> EnergyResourceTypes { get { return AppDatabase.Instance.ResourceTypes; } }
        public MyICommand CreateCommand { get; private set; }
        public MyICommand DeleteCommand { get; private set; }
        public MyICommand SearchCommand { get; private set; }
        public MyICommand ClearSearchCommand { get; private set; }
        #endregion

        public NetworkEntitiesViewModel() 
        {
            CreateCommand = new MyICommand(OnCreateCommand);
            DeleteCommand = new MyICommand(OnDeleteCommand);
            SearchCommand = new MyICommand(OnSearchCommand);
            ClearSearchCommand = new MyICommand(OnClearSearchCommand, CanClearSearch);

            EnergyResources = CollectionViewSource.GetDefaultView(AppDatabase.Instance.Resources);
            EnergyResources.Filter = ResourceFilter;
        }

        public bool ResourceFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchResourceName) && SearchResourceType == null)
            {
                FilterActive = false;
                return true;
            }

            if (item is DistributedEnergyResource resource)
            {
                if(SearchByName)
                {
                    if (string.IsNullOrEmpty(SearchResourceName)) return true;
                    FilterActive = true;
                    return resource.Name.ToLower().Contains(SearchResourceName.ToLower());
                }
                else
                {
                    if (SearchResourceType == null) return true;
                    FilterActive = true;
                    return resource.Type == SearchResourceType;
                }
            }
            return false;
        }

        #region Command Implementations
        public async void OnCreateCommand()
        {
            CreateEnergyResource.Validate();
            if(CreateEnergyResource.IsValid)
            {
                var canCreate = await GetApproval($"Are you sure you want to create resource  {CreateEnergyResource.Name}?");
                if (canCreate)
                {

                    DistributedEnergyResource newResource = new DistributedEnergyResource(0, CreateEnergyResource.Name, CreateEnergyResource.Type, null);
                    AppDatabase.Instance.AddResource(newResource);

                    CreateEnergyResource.Name = "";
                    CreateEnergyResource.Type = null;
                }
            }
            else
            {
                AppDatabase.Instance.ActionFailiure("Faield to add resource.");
            }
        }
        public async void OnDeleteCommand()
        {
            if(SelectedResource != null)
            {
                var toDeleteId = SelectedResource.Id;
                
                var canDelete = await GetApproval($"Are you sure you want to delete resoruce {SelectedResource.Name}?");
                if (canDelete)
                {
                    AppDatabase.Instance.RemoveResource(toDeleteId);
                }
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
            FilterActive = false;

            EnergyResources.Refresh();
        }

        private bool CanClearSearch() => FilterActive;
        #endregion

        private async Task<bool> GetApproval(string message)
        {
            var mainVM = App.Current.MainWindow.DataContext as MainWindowViewModel;

            return await mainVM.ShowConfirmDialog(message);
        }
    }
}
