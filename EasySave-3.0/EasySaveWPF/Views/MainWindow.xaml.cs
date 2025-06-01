using EasySave.Model;
using EasySaveWPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EasySaveWPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

    
    private void BackupListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectedBackups.Clear();
                foreach (var item in BackupListView.SelectedItems.Cast<Backup>())
                {
                    vm.SelectedBackups.Add(item);
                }
            }
        }

    }
}