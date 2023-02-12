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
        }

        MainViewModel ViewModel => (DataContext as MainViewModel)!;
    }
}
