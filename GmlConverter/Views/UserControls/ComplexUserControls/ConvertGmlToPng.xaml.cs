using GmlConverter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GmlConverter.Views.UserControls
{
	/// <summary>
	/// Interaction logic for GmlToPng.xaml
	/// </summary>
	public partial class ConvertGmlToPng : UserControl
	{
		private ConvertGmlToPngViewModel _vm;

		public ConvertGmlToPng()
		{
			_vm = new();
			DataContext = _vm;
			InitializeComponent();
		}

		private void InactiveFilterCheckBox_Changed(object sender, RoutedEventArgs e)
		{
			// Refresh the view to apply filters.
			CollectionViewSource.GetDefaultView(gmlFileInformationDataGrid.ItemsSource).Refresh();
		}

		private void GroupingCheckBox_Changed(object sender, RoutedEventArgs e)
		{
			if (gmlFileInformationDataGrid == null)
				return;
			var cvTasks = CollectionViewSource.GetDefaultView(gmlFileInformationDataGrid.ItemsSource);
			if (cvTasks == null)
				return;
			
			if (groupingCheckBox.IsChecked?? false)
			{
				if (cvTasks.CanGroup)
				{
					cvTasks.GroupDescriptions.Clear();
					cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("MeshArea1"));
					cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("MeshArea2"));
				}
			}
			else
			{
				cvTasks.GroupDescriptions.Clear();
			}
		}

		private void CvsGmlFileInformations_Filter(object sender, FilterEventArgs e)
		{
			///項目フィルタリング処理。
			var taskWhenAll = e.Item as GmlFileInformation;
			if (taskWhenAll == null)
				return;
			// If filter is turned on, filter inactive items.
			e.Accepted = !((inactiveFilterCheckBox.IsChecked?? false) && !taskWhenAll.Active);
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);
				_vm.AddGmlFiles(files);
			}
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e) => _vm.Load();
		private void SaveButton_Click(object sender, RoutedEventArgs e) => _vm.Save();

		private void GmlFileInformationDataGrid_ContextMenu_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (gmlFileInformationDataGrid.SelectedItems.Count <= 0)
				return;
			
			var selectedItems = gmlFileInformationDataGrid.SelectedItems.Cast<GmlFileInformation>().ToList();
			if (selectedItems == null)
				return;
			
			_vm.RemoveRows(selectedItems);
		}

    }
}
