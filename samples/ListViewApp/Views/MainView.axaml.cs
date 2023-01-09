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

            //list2.SelectionChanged += List_SelectionChanged;
        }

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

        }

        private void BtnDeselect_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

        }
    }
}
