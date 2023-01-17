using Avalonia.Controls.Primitives;
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
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using System.Xml.Linq;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Layout;

namespace ListViewApp.Controls;

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
    IEnumerable? _ncc_items;
    UnnotifyList? _ncclist;

    /// <summary>
    /// Gets or sets the items to display.
    /// </summary>
    [Content]
    public IEnumerable? Items
    {
        get { return _items; }
        set
        {
            if (ReferenceEquals(_items, value))
                return;

            if (value is INotifyCollectionChanged ncc)
            {
                _ncc_items = value;
                value = _ncclist = new UnnotifyList((value as IList)!);
                ncc.CollectionChanged += Ncc_CollectionChanged;
            }

            SetAndRaise(ItemsProperty, ref _items, value);
        }
    }

    public ListView()
    {
        AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture);
        AddHandler(Gestures.ScrollGestureEndedEvent, OnScrollGestureEnded);
    }

    double stretch_y = 1.0;
    private void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnScrollGesture ({e.Id}): {e.Delta.Y}");

        stretch_y += (Math.Abs(e.Delta.Y) / 8600.0);

        e.Handled = true;
        e.ShouldEndScrollGesture = true;

        StretchRepeaterVertically(stretch_y);

        //double z = 1.0;

        //double step = 0.010;

        //double stretch_end = 0.05;

        //double stretch = 0.0;// _stretchController.value;
        //int direction = 1;

        //DispatcherTimer.Run(
        //    () =>
        //    {
        //        double x = 1.0;
        //        double y = 1.0;


        //        y += stretch;

        //        _repeater.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        //        _repeater.RenderTransform = new Avalonia.Media.MatrixTransform(new Matrix(
        //        x, 0.0, 0.0,
        //        0.0, y, 0.0,
        //        0.0, 0.0, z));

        //        stretch += step * direction;

        //        if (stretch <= 0.0)
        //            return false;

        //        if (direction > 0 && stretch > stretch_end)
        //        {
        //            direction = -direction;
        //        }

        //        return true;
        //    }, TimeSpan.FromMilliseconds(16));
    }

    void StretchRepeaterVertically(double value)
    {
        _repeater.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        _repeater.RenderTransform = new Avalonia.Media.MatrixTransform(new Matrix(
        1.0, 0.0, 0.0,
        0.0, value, 0.0,
        0.0, 0.0, 1.0));
    }

    void ReleaseStretchRepeater()
    {
        var steps = (stretch_y - 1.0) / 3;

        DispatcherTimer.Run(
            () =>
            {
                stretch_y -= steps;

                StretchRepeaterVertically(stretch_y);

                if (stretch_y <= 1.0)
                {
                    StretchRepeaterVertically(1.0);
                    return false;
                }

                return true;
            }, TimeSpan.FromMilliseconds(16));
    }

    private void OnScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnScrollGestureEnded ({e.Id})");
        ReleaseStretchRepeater();
    }

    private class UnnotifyList : IList, INotifyCollectionChanged
    {
        IList _items;

        public UnnotifyList(IList items)
        {
            _items = new ArrayList(items);
        }

        public object? this[int index] { get => _items[index]; set => _items[index] = value; }

        public bool IsFixedSize => _items.IsFixedSize;

        public bool IsReadOnly => _items.IsReadOnly;

        public int Count => _items.Count;

        public bool IsSynchronized => _items.IsSynchronized;

        public object SyncRoot => _items.SyncRoot;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public int Add(object? value)
        {
            return _items.Add(value);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(object? value)
        {
            return _items.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            _items.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(object? value)
        {
            return _items.IndexOf(value);
        }

        public void Insert(int index, object? value)
        {
            _items.Insert(index, value);
        }

        public void Notify(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public void Remove(object? value)
        {
            _items.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }
    }

    const double AnimationDuration = 125;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        //InteractiveParent!.AddHandler
    }

    void BringIndexIntoView(int index)
    {
        var layoutManager = ((TopLevel)VisualRoot!).LayoutManager;
        var buitenBeeld = _repeater.GetOrCreateElement(index);

        if (buitenBeeld != null)
        {
            layoutManager.ExecuteLayoutPass();
            buitenBeeld.BringIntoView();
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Android voert deze arrange uit na toevoegen van de addedElement, en zet deze dan top
        // hier gehackt: child visuals met class TEST overslaan

        var arrangeRect = new Rect(finalSize);

        var visualChildren = VisualChildren;
        var visualCount = visualChildren.Count;

        for (var i = 0; i < visualCount; i++)
        {
            Visual visual = visualChildren[i];
            if (visual is Layoutable layoutable && !visual.Classes.Contains("TEST"))
            {
                layoutable.Arrange(arrangeRect);
            }
        }

        return finalSize;
    }

    Control? addedElement;

    private void Ncc_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            
            int index = e.NewStartingIndex;
            object? addedObject = e.NewItems![0];

            BringIndexIntoView(Math.Max(0, index - 1));
            //_scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled; // scrolling while animating the items is probably not a good idea?

            

            Dispatcher.UIThread.Post(() =>
            {
                var previousElement = _repeater.TryGetElement(index - 1);
                System.Diagnostics.Debug.WriteLine($"previousElement!.Bounds.BottomLeft = {previousElement!.Bounds.BottomLeft}");
                var bottomLeftScreen = _repeater.PointToScreen(previousElement!.Bounds.BottomLeft);
                System.Diagnostics.Debug.WriteLine($"bottomLeftScreen = {bottomLeftScreen.Y}");
                var bottomLeftListView = this.PointToClient(bottomLeftScreen);

                System.Diagnostics.Debug.WriteLine($"bottomLeftListView = {bottomLeftListView.Y}");
                double top = bottomLeftListView.Y;// previousElement?.Bounds.Bottom ?? -1.0;
                double addedElementDesiredHeight = 0.0;

                addedElement = _repeater.ItemTemplate!.Build(addedObject);
                if (addedElement is not null)
                {
                    addedElement.Classes.Add("TEST");
                    //(addedElement as ListBoxItem)!.Background = new SolidColorBrush(Colors.Red);
                    addedElement.DataContext = addedObject;
                    
                    LogicalChildren.Add(addedElement);
                    //_repeater.Children.Add(addedElement!);
                    //addedElement.ApplyStyling();
                    addedElement.Measure(Size.Infinity);
                    
                    
                    
                    System.Diagnostics.Debug.WriteLine($"addedElement.DesiredSize.Height = {addedElement.DesiredSize.Height}");
                    addedElementDesiredHeight = addedElement.DesiredSize.Height;
                    addedElement.Arrange(new Rect(new Point(0.0, top), new Size(Bounds.Width, addedElementDesiredHeight)));
                    //addedElement.Opacity = 0.0;
                    //addedElement.ApplyTemplate();
                    //addedElement.ApplyStyling();

                    VisualChildren.Add(addedElement);

                    //LogicalChildren.Remove(addedElement);
                    System.Diagnostics.Debug.WriteLine($"addedElement.Bounds = {addedElement.Bounds}");
                    StartOpacityAnimationOn(addedElement);
                }

                Control? element;
                while ((element = _repeater.TryGetElement(index++)) != null)
                {
                    var compositionVisual = (ElementComposition.GetElementVisual(element) as CompositionContainerVisual)!;
                    var compositor = compositionVisual.Compositor;
                    // "Offset" is a Vector3 property, so we create a Vector3KeyFrameAnimation
                    var animation = compositor.CreateVector3KeyFrameAnimation();
                    var a = compositor.CreateQuaternionKeyFrameAnimation();
                    // Change the offset of the visual slightly to the left when the animation beginning
                    //new Quaternion()
                    // Revert the offset to the original position (0,0,0) when the animation ends
                    animation.InsertKeyFrame(0f, compositionVisual.Offset, new Avalonia.Animation.Easings.CubicEaseOut());
                    animation.InsertKeyFrame(1f, compositionVisual.Offset with { Y = compositionVisual.Offset.Y + (float)addedElementDesiredHeight }, new Avalonia.Animation.Easings.CubicEaseOut());

                    animation.StopBehavior = Avalonia.Rendering.Composition.Animations.AnimationStopBehavior.SetToFinalValue;
                    animation.Duration = TimeSpan.FromMilliseconds(AnimationDuration);
                    // Start the new animation!
                    compositionVisual.StartAnimation("Offset", animation);
                }

                DispatcherTimer.RunOnce(() =>
                {
                    if (addedElement is not null)
                    {
                        System.Diagnostics.Debug.WriteLine("REMOVE addedElement");
                        VisualChildren.Remove(addedElement);
                        //LogicalChildren.Remove(addedElement);
                        addedElement = null;
                    }

                    _ncclist!.Insert(e.NewStartingIndex, e.NewItems![0]!);
                    (_items as UnnotifyList)!.Notify(e);
                    //_repeater.ElementPrepared += _repeater_ElementPrepared1;

                    //_scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }, TimeSpan.FromMilliseconds(AnimationDuration));

            }, priority: DispatcherPriority.Layout);
        }
        
    }

    private void _repeater_ElementPrepared1(object? sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        //StartOpacityAnimationOn(e.Element);
        //_repeater.ElementPrepared -= _repeater_ElementPrepared1;
    }

    void StartOpacityAnimationOn(Control element)
    {
        var compositionVisual = (ElementComposition.GetElementVisual(element) as CompositionContainerVisual)!;
        var compositor = compositionVisual.Compositor;
        // "Offset" is a Vector3 property, so we create a Vector3KeyFrameAnimation
        var animation = compositor.CreateScalarKeyFrameAnimation();
        // Change the offset of the visual slightly to the left when the animation beginning

        // Revert the offset to the original position (0,0,0) when the animation ends
        animation.InsertKeyFrame(0f, 0, new Avalonia.Animation.Easings.CubicEaseOut());
        animation.InsertKeyFrame(1f, 1, new Avalonia.Animation.Easings.CubicEaseIn());

        animation.StopBehavior = Avalonia.Rendering.Composition.Animations.AnimationStopBehavior.SetToFinalValue;
        animation.Duration = TimeSpan.FromMilliseconds(AnimationDuration);
        // Start the new animation!
        compositionVisual.StartAnimation("Opacity", animation);
    }

    private void Element_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
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
    ScrollViewer _scrollViewer = default!;

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

        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer")!;
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
