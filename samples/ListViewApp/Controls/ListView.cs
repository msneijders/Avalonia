﻿using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using System.Collections.Specialized;

namespace ListViewApp.Controls
{
    public partial class ListView : TemplatedControl
    {
        /// <summary>
        /// Defines the <see cref="SelectedIndex"/> property.
        /// </summary>
        public static readonly DirectProperty<ListView, int> SelectedIndexProperty =
            AvaloniaProperty.RegisterDirect<ListView, int>(
                nameof(SelectedIndex),
                o => o.SelectedIndex,
                (o, v) => o.SelectedIndex = v,
                unsetValue: -1,
                defaultBindingMode: BindingMode.TwoWay);

        /// <summary>
        /// Defines the <see cref="SelectedItem"/> property.
        /// </summary>
        public static readonly DirectProperty<ListView, object?> SelectedItemProperty =
            AvaloniaProperty.RegisterDirect<ListView, object?>(
                nameof(SelectedItem),
                o => o.SelectedItem,
                (o, v) => o.SelectedItem = v,
                defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

        /// <summary>
        /// Defines the <see cref="Items"/> property.
        /// </summary>
        public static readonly DirectProperty<ListView, IEnumerable?> ItemsProperty =
            AvaloniaProperty.RegisterDirect<ListView, IEnumerable?>(nameof(Items), o => o.Items, (o, v) => o.Items = v);

        /// <summary>
        /// Defines the <see cref="ItemTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
            AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(ItemTemplate));

        /// <summary>
        /// Defines the <see cref="SelectionChanged"/> event.
        /// </summary>
        public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
            RoutedEvent.Register<ListView, SelectionChangedEventArgs>(
                nameof(SelectionChanged),
                RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the control's selection changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        IEnumerable? _items;

        /// <summary>
        /// Gets or sets the items to display.
        /// </summary>
        [Content]
        public IEnumerable? Items
        {
            get { return _items; }
            set
            {
                if (SetAndRaise(ItemsProperty, ref _items, value))
                {
                    if (_items is INotifyCollectionChanged ncc)
                    {
                        ncc.CollectionChanged += Ncc_CollectionChanged;
                    }
                }
            }
        }

        private void Ncc_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                Control? element;
                int index = e.NewStartingIndex;
                while ((element = _repeater.TryGetElement(index++)) != null)
                {
                    element.RenderTransformOrigin = new RelativePoint(0.0, 4.0, RelativeUnit.Relative);
                }
            }
            
        }

        /// <summary>
        /// Gets or sets the data template used to display the items in the control.
        /// </summary>
        public IDataTemplate? ItemTemplate
        {
            get { return GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        int _selectedIndex = -1;

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                // When a Begin/EndInit/DataContext update is in place we return the value to be
                // updated here, even though it's not yet active and the property changed notification
                // has not yet been raised. If we don't do this then the old value will be written back
                // to the source when two-way bound, and the update value will be lost.
                return _selectedIndex;
            }
            set
            {
                SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value);
            }
        }

        object? _selectedItem;
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object? SelectedItem
        {
            get
            {
                // See SelectedIndex setter for more information.
                return _selectedItem;
            }
            set
            {
                if (SetAndRaise(SelectedItemProperty, ref _selectedItem, value))
                {
                    var index = (Items as IList)!.IndexOf(value);
                    UpdateSelectedObject(index, value);
                }
            }
        }

        ItemsRepeater _repeater = default!;
        private PointerPoint _pointPressed;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.Source is Visual source)
            {
                _pointPressed = e.GetCurrentPoint(source);
            }
        }

        void UpdateSelectedObject(int index, object? obj)
        {
            var _oldSelected = SelectedIndex >= 0 ? _repeater.TryGetElement(SelectedIndex) as ListBoxItem : null;
            object[]? oldSelectedItems = null;
            object[]? newSelectedItems = null;
            if (_oldSelected is not null)
            {
                _oldSelected.IsSelected = false;
                oldSelectedItems = new object[] { _oldSelected.DataContext! };
            }

            if (index >= 0)
            {
                SelectedIndex = index;
                var newSelected = _repeater.TryGetElement(index) as ListBoxItem;
                
                if (newSelected is not null)
                {
                    newSelected.IsSelected = true;
                    newSelected.BringIntoView();
                } else
                {
                    var layoutManager = ((TopLevel)VisualRoot!).LayoutManager;
                    var buitenBeeld = _repeater.GetOrCreateElement(index);
                    
                    if (buitenBeeld != null)
                    {
                        layoutManager.ExecuteLayoutPass();
                        buitenBeeld.BringIntoView();
                    }
                }

                newSelectedItems = obj is not null ? new object[] { obj! } : null;
            }

            RaiseEvent(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                oldSelectedItems ?? Array.Empty<object>(),
                newSelectedItems ?? Array.Empty<object>()));
        }

        void UpdateSelectedObject(ListBoxItem? item)
        {
            if (_selectedItem == item) return;

            var _oldSelected = SelectedIndex >= 0 ? _repeater.TryGetElement(SelectedIndex) as ListBoxItem : null;
            _selectedItem = item;

            object[]? oldSelectedItems = null;
            object[]? newSelectedItems = null;
            if (_oldSelected is not null)
            {
                _oldSelected.IsSelected = false;
                oldSelectedItems = new object[] { _oldSelected.DataContext! };
            }

            if (item is not null)
            {
                item.IsSelected = true;

                newSelectedItems = new object[] { item.DataContext! };
                item.BringIntoView();

                SelectedIndex = _repeater.GetElementIndex(item);
                SelectedItem = item.DataContext;
            }

            RaiseEvent(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                oldSelectedItems ?? Array.Empty<object>(),
                newSelectedItems ?? Array.Empty<object>()));
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (e.Source is Visual)
            {
                if (_pointPressed.Properties.IsLeftButtonPressed)
                {
                    var container = GetContainerFromEventSource(e.Source);
                    if (container is ListBoxItem lbi)
                    {
                        
                        UpdateSelectedObject(lbi);
                        //object[]? oldSelectedItems = null;
                        //object[]? newSelectedItems = null;
                        //if (SelectedIndex >= 0)
                        //{
                        //    var _oldSelected = _repeater.TryGetElement(SelectedIndex) as ListBoxItem;
                        //    if (_oldSelected is not null)
                        //    {
                        //        _oldSelected.IsSelected = false;
                        //        oldSelectedItems = new object[] { _oldSelected.DataContext! };
                        //    }
                        //}

                        //lbi.IsSelected = true;

                        //newSelectedItems = new object[] { lbi.DataContext! };


                        //SelectedIndex = _repeater.GetElementIndex(lbi);
                        //SelectedItem = lbi.DataContext;

                        //RaiseEvent(new SelectionChangedEventArgs(
                        //    SelectionChangedEvent,
                        //    oldSelectedItems ?? Array.Empty<object>(),
                        //    newSelectedItems));

                        e.Handled = true;
                    }
                }
            }
        }

        protected Control? GetContainerFromEventSource(object? eventSource)
        {
            for (var current = eventSource as Visual; current != null; current = current.Parent as Visual)
            {
                if (current is Control control && control.Parent == _repeater)
                {
                    return control;
                }
            }

            return null;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _repeater = e.NameScope.Find<ItemsRepeater>("PART_ItemsPresenter")!;
            _repeater.ElementClearing += _repeater_ElementClearing;
            _repeater.ElementPrepared += _repeater_ElementPrepared;
        }

        private void _repeater_ElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
        {
            if (e.Element is ListBoxItem lbi)
            {
                lbi.IsSelected = SelectedIndex == e.Index;
            }
        }

        private void _repeater_ElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs e)
        {
            if (e.Element is ListBoxItem lbi)
            {
                lbi.IsSelected = false;
            }
        }

        public void UnselectAll()
        {
            var selectedIndex = SelectedIndex;

            SelectedIndex = -1;
            SelectedItem = null;

            object[]? oldSelectedItems = null;
            if (selectedIndex >= 0)
            {
                var _oldSelected = _repeater.TryGetElement(selectedIndex) as ListBoxItem;
                if (_oldSelected is not null)
                {
                    _oldSelected.IsSelected = false;
                    oldSelectedItems = new object[] { _oldSelected.DataContext! };
                }
            }

            RaiseEvent(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                oldSelectedItems ?? Array.Empty<object>(),
                Array.Empty<object>()));
        }
    }
}
