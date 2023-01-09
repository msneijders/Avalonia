using Avalonia;
using Avalonia.Controls;
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

        }

        private void Insert_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel.InsertRegel();
        }
    }
}
