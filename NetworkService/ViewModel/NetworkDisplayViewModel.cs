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
using System.Windows.Input;
using System.Windows.Media;
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
        private int _sourceSlotIdx = -1;
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
            set => SetProperty(ref _selectedResource, value);
        }
        public MyICommand<int> RemoveFromGridCommand { get; set; }
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

            RemoveFromGridCommand = new MyICommand<int>(OnRemoveFromGrid);
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

        #region DragAndDrop Events
        public void OnDragStart(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is DistributedEnergyResource resource)
            {
                SelectedResource = resource;
                DragDrop.DoDragDrop((DependencyObject)sender, resource, DragDropEffects.Move);
            }
        }

        public void OnGridDragStart(object sender, MouseButtonEventArgs e)
        {
            DependencyObject element = e.OriginalSource as DependencyObject;
            ContentControl contentControl = null;

            while (element != null)
            {
                if (element is ContentControl cc)
                {
                    contentControl = cc;
                    break;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            if (contentControl != null && contentControl.Content is DistributedEnergyResource resource)
            {
                if (contentControl.Tag != null && int.TryParse(contentControl.Tag.ToString(), out int slotIdx))
                {
                    SelectedResource = resource;
                    _sourceSlotIdx = slotIdx;
                    DragDrop.DoDragDrop((DependencyObject)sender, resource, DragDropEffects.Move);
                }
            }
        }

        public void OnDrop(object sender, DragEventArgs e)
        {
            if(SelectedResource != null)
            {
                DependencyObject element = e.OriginalSource as DependencyObject;
                ContentControl contentControl = null;

                while (element != null)
                {
                    if (element is ContentControl cc)
                    {
                        contentControl = cc;
                        break;
                    }
                    element = VisualTreeHelper.GetParent(element);
                }

                if (contentControl.Tag != null && int.TryParse(contentControl.Tag.ToString(), out int slotIdx))
                {
                    if (_sourceSlotIdx != -1)
                    {
                        if (_sourceSlotIdx != slotIdx)
                        {
                            AppDatabase.Instance.MoveResourceOnGrid(_sourceSlotIdx, slotIdx);
                        }
                    }
                    else
                    {
                        AppDatabase.Instance.PlaceResourceOnGrid(SelectedResource, slotIdx);
                    }
                }

                SelectedResource = null;
                _sourceSlotIdx = -1;
            }
        }
        #endregion

        private void OnRemoveFromGrid(int id)
        {
            int idx = -1;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].Id == id)
                {
                    idx = i;
                    break;
                }
            }

            if(idx != -1)
            {
                AppDatabase.Instance.RemoveResourceFromGrid(idx);
            }
        }
    }
}
