using NetworkService.Model;
using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
        private bool _drawMode = false;
        private int _firstSelectedSlotIdx = -1;

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
        public bool DrawMode
        {
            get => _drawMode;
            set => SetProperty(ref _drawMode, value);
        }
        public MyICommand<int> RemoveFromGridCommand { get; set; }
        public MyICommand DrawModeChangeCommand { get; set; }
        public GridSlot[] Slots { get => AppDatabase.Instance.GridSlots; }
        public ObservableCollection<LineConnection> Connections { get => AppDatabase.Instance.Connections; }
        #endregion

        public NetworkDisplayViewModel() 
        {
            RefreshTreeView();

            AppDatabase.Instance.Resources.CollectionChanged += Resource_CollectionChanged;
            AppDatabase.Instance.PropertyChanged += Database_PropertyChanged;

            RemoveFromGridCommand = new MyICommand<int>(OnRemoveFromGrid);
            DrawModeChangeCommand = new MyICommand(OnDrawModeChanged);
        }

        #region Event subscribers
        private void Resource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTreeView();
            OnPropertyChanged(nameof(Slots));
        }
        private void Database_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppDatabase.GridSlots))
            {
                RefreshTreeView();
                OnPropertyChanged(nameof(Slots));
            }
        }
        #endregion

        public void RefreshTreeView()
        {
            var availableResource = AppDatabase.Instance.Resources
                .Where(r => !AppDatabase.Instance.GridSlots.Any(slot => slot.Resource != null && slot.Resource.Id == r.Id));

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
            if (DrawMode)
                return;

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

            if (contentControl != null && contentControl.Content is GridSlot slot)
            {
                if (contentControl.Tag != null && int.TryParse(contentControl.Tag.ToString(), out int slotIdx))
                {
                    if(DrawMode)
                    {
                        if (slot.Resource != null)
                        {
                            HandleSlotClickInDrawMode(slotIdx);
                        }
                        return;
                    }

                    if (slot.Resource != null)
                    {
                        SelectedResource = slot.Resource;
                        _sourceSlotIdx = slotIdx;
                        DragDrop.DoDragDrop((DependencyObject)sender, slot.Resource, DragDropEffects.Move);
                    }
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
                if (Slots[i].Resource != null && Slots[i].Resource.Id == id)
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

        private void HandleSlotClickInDrawMode(int clickedSlotIdx)
        {
            if (_firstSelectedSlotIdx == -1)
            {
                _firstSelectedSlotIdx = clickedSlotIdx;
                Slots[clickedSlotIdx].IsSelected = true;
            }
            else
            {
                int secondSelectedSlotIdx = clickedSlotIdx;

                if (_firstSelectedSlotIdx != secondSelectedSlotIdx)
                {
                    Slots[_firstSelectedSlotIdx].IsSelected = false;
                    Slots[secondSelectedSlotIdx].IsSelected = false;
                    AppDatabase.Instance.ConnectResourcesOnGrid(_firstSelectedSlotIdx, secondSelectedSlotIdx);
                }
                else
                {
                    Slots[clickedSlotIdx].IsSelected = false;
                }
                _firstSelectedSlotIdx = -1;
            }
        }

        private void OnDrawModeChanged()
        {
            DrawMode = !DrawMode;
            if (!DrawMode)
            {
                foreach (var slot in Slots)
                {
                    slot.IsSelected = false;
                }
            }
        }
    }
}
