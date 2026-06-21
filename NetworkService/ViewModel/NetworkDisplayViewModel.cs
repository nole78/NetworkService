using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace NetworkService.ViewModel
{
    public class TreeViewTypeGroup
    {
        public string TypeName { get; set; }
        public List<DistributedEnergyResource> Resources { get; set; }
    }
    public class NetworkDisplayViewModel : BindableBase
    {
        private List<TreeViewTypeGroup> _treeViewNodes;
        public DistributedEnergyResource[] Slots => AppDatabase.Instance.GridSlots;

        #region Properties
        public List<TreeViewTypeGroup> TreeViewNodes 
        {
            get => _treeViewNodes;
            set => SetProperty(ref _treeViewNodes, value); 
        }
        #endregion
        
        public NetworkDisplayViewModel() 
        {
            RefreshTreeView();

            AppDatabase.Instance.Resources.CollectionChanged += Resource_CollectionChanged;
        }

        #region Event subscribers
        private void Resource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshTreeView();

            OnPropertyChanged(nameof(Slots));
        }
        #endregion

        public void RefreshTreeView()
        {
            TreeViewNodes = AppDatabase.Instance.Resources.GroupBy(r => r.Type.Name)
                .Select(group => new TreeViewTypeGroup
                {
                    TypeName = group.Key,
                    Resources = group.ToList()
                }).ToList();
        }
    }
}
