using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private DistributedEnergyResource _selectedResource;
        public DistributedEnergyResource[] Slots => AppDatabase.Instance.GridSlots;

        #region Properties
        public List<TreeViewTypeGroup> TreeViewNodes 
        {
            get => _treeViewNodes;
            set => SetProperty(ref _treeViewNodes, value); 
        }
        public DistributedEnergyResource SelectedResource
        {
            get => _selectedResource;
            set =>SetProperty(ref _selectedResource, value);
        }
        #endregion
        
        public NetworkDisplayViewModel() 
        {
            RefreshTreeView();

            AppDatabase.Instance.Resources.CollectionChanged += Resource_CollectionChanged;
            AppDatabase.Instance.PropertyChanged += (s, e) => 
            { 
                if (e.PropertyName == nameof(AppDatabase.GridSlots)) 
                { 
                    RefreshTreeView(); 
                    OnPropertyChanged(nameof(Slots)); 
                } 
            };
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
            var availableResource = AppDatabase.Instance.Resources
                .Where(r => !AppDatabase.Instance.GridSlots.Any(slot => slot != null && slot.Id == r.Id));

            TreeViewNodes = availableResource.GroupBy(r => r.Type.Name)
                .Select(group => new TreeViewTypeGroup
                {
                    TypeName = group.Key,
                    Resources = group.ToList()
                }).ToList();
        }

        public void OnDragStart(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is DistributedEnergyResource resource)
            {
                SelectedResource = resource;
                DragDrop.DoDragDrop((DependencyObject)sender, resource, DragDropEffects.Move);
            }
        }

        public void OnDrop(object sender, DragEventArgs e)
        {
            if(SelectedResource != null && sender is ContentControl contentControl)
            {
                if(contentControl.Tag != null && int.TryParse(contentControl.Tag.ToString(), out int slotIdx))
                {
                    AppDatabase.Instance.PlaceResourceOnGrid(SelectedResource, slotIdx);
                }

                SelectedResource = null;
            }
        }
    }
}
