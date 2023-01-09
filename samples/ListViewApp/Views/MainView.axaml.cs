using Avalonia;
using Avalonia.Controls;

namespace ListViewApp.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            btnDeselect.Click += BtnDeselect_Click;
            btnTest.Click += BtnTest_Click;

            list2.SelectionChanged += List_SelectionChanged;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            bottomSheet.Width = result.Width / 2.0;
            bottomSheet2.Width = result.Width / 2.0;
            Canvas.SetLeft(bottomSheet2, result.Width / 2.0);
            return result;
        }

        private void List_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (list2.SelectedItem is null)
            {
                bottomSheet.Classes.Clear();
                bottomSheet2.Classes.Clear();
            }
            else
            {
                if(!bottomSheet.Classes.Contains("open"))
                {
                    bottomSheet.Classes.Add("open");
                    bottomSheet2.Classes.Add("open");
                }
            }
            
        }

        private void BtnTest_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //bottomSheet.RenderTransformOrigin = new Avalonia.RelativePoint();
            bottomSheet.Classes.Clear();
            bottomSheet.Classes.Add("open");

            bottomSheet2.Classes.Clear();
            bottomSheet2.Classes.Add("open");
        }

        private void BtnDeselect_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            list2.UnselectAll();
        }
    }
}
