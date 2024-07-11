using GmlConverter.Utilities;
using GmlConverter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GmlConverter.Views.UserControls
{
    /// <summary>
    /// DevidePng.xaml の相互作用ロジック
    /// </summary>
    public partial class DevidePng : UserControl, IDisposable
	{
		private ScrollScaleImageController _previewController;
		private DevidePngViewModel _vm;

		public DevidePng()
		{
			_previewController = new();
			_vm = new();
			DataContext = _vm;
			InitializeComponent();
			Loaded += CreateMask_Loaded;
		}

		~DevidePng() => Dispose();

		public void Dispose()
		{
			_vm.Dispose();
		}

		private void CreateMask_Loaded(object sender, RoutedEventArgs e)
		{
			var imageScale = Resources["ImageScale"] as ScaleTransform;
			if(imageScale == null)
			{
				throw new Exception();
			}
			_previewController.Loaded(previewImage, imageScale, previewScrollVewer);
		}

		private void CommandBinding_PreviewScaleChange(object sender, ExecutedRoutedEventArgs e) => _previewController.PreviewScaleChange(sender, e);
		private void PreviewImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => _previewController.MouseRightButtonDown(sender, e);
		private void PreviewImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e) => _previewController.MouseRightButtonUp(sender, e);
		private void PreviewImage_MouseMove(object sender, MouseEventArgs e) => _previewController.MouseMove(sender, e);
		private void PreviewImage_MouseWheel(object sender, MouseWheelEventArgs e) => _previewController.MouseWheel(sender, e);

		private void LoadButton_Click(object sender, RoutedEventArgs e) => _vm.Load();
		private void SaveButton_Click(object sender, RoutedEventArgs e) => _vm.Save();

		private void FileInformationDataGrid_ContextMenu_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (fileInformationDataGrid.SelectedItems.Count <= 0)
				return;

			var selectedItems = fileInformationDataGrid.SelectedItems.Cast<DevidePngFileInformation>().ToList();
			if (selectedItems == null)
				return;

			_vm.RemoveRows(selectedItems);

		}

		private void PreviewImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var position = e.GetPosition(previewImage);
			_vm.SetCenterPoint(new((int)position.X, (int)position.Y));
		}
	}
}
