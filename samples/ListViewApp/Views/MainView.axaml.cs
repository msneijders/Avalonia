using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ListViewApp.ViewModels;

namespace ListViewApp.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            btnInsert.Click += Insert_Click;
            btnTest.Click += BtnTest_Click;

            //list2.SelectionChanged += List_SelectionChanged;
        }

        MainViewModel ViewModel => (DataContext as MainViewModel)!;

        protected override void OnLoaded()
        {
            base.OnLoaded();

            
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            return result;
        }

        private void List_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            
        }

        private void BtnTest_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            double z = 1.0;

            double step = 0.010;

            double stretch_end = 0.05;

            double stretch = 0.0;// _stretchController.value;
            int direction = 1;

            DispatcherTimer.Run(
                () =>
                {
                    double x = 1.0;
                    double y = 1.0;


                    y += stretch;

                    list.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
                    list.RenderTransform = new Avalonia.Media.MatrixTransform(new Matrix(
                    x, 0.0, 0.0,
                    0.0, y, 0.0,
                    0.0, 0.0, z));

                    stretch += step * direction;

                    if (stretch <= 0.0)
                        return false;

                    if (direction > 0 && stretch > stretch_end)
                    {
                        direction = -direction;
                    }

                    return true;
                }, TimeSpan.FromMilliseconds(16));
        }

        private void Insert_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel.InsertRegel();
        }
    }
}
