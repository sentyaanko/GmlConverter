using GmlConverter.Utilities;
using GmlConverter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GmlConverter.Views.UserControls
{
	/// <summary>
	/// NoDataToSlope.xaml の相互作用ロジック
	/// </summary>
	public partial class NoDataToSlope : UserControl
	{
		private ScrollScaleImageController _previewController;
		private NoDataToSlopeViewModel _vm;

		public NoDataToSlope()
		{
			_previewController = new();
			_vm = new();
			DataContext = _vm;
			InitializeComponent();
			Loaded += CreateMask_Loaded;
		}
		private void CreateMask_Loaded(object sender, RoutedEventArgs e) => _previewController.Loaded(previewImage, imageScale, previewScrollVewer);
		private void CommandBinding_PreviewScaleChange(object sender, ExecutedRoutedEventArgs e) => _previewController.PreviewScaleChange(sender, e);
		private void PreviewImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => _previewController.MouseRightButtonDown(sender, e);
		private void PreviewImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e) => _previewController.MouseRightButtonUp(sender, e);
		private void PreviewImage_MouseMove(object sender, MouseEventArgs e) => _previewController.MouseMove(sender, e);
		private void PreviewImage_MouseWheel(object sender, MouseWheelEventArgs e) => _previewController.MouseWheel(sender, e);

		private void LoadButton_Click(object sender, RoutedEventArgs e) => _vm.Load();
		private void SaveButton_Click(object sender, RoutedEventArgs e) => _vm.Save();

		private void SlopeApplyButton_Click(object sender, RoutedEventArgs e)
		{
			_vm.SlopeApply();

		}
    }
}
