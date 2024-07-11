using GmlConverter.Utilities;
using GmlConverter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;


/*
# Effect を使用してマスク画像を描画する方法に関する調査結果

いまのところの、表示がマシな組み合わせ

* Image.RenderOptions.BitmapScalingMode="NearestNeighbor" を指定する
	* これ以外を指定すると、イメージの拡大縮小時に下位バイトの 255 - 0 の切り替わる場所を検出してしまう。
	* NearestNeighbor ではそれは発生しない。（が、格子状にラインが入ってしまうが、幾分マシ）
* RegisterPixelShaderSamplerProperty() の第四引数で SamplingMode.Bilinear を指定する。
	* これ以外を指定すると、イメージの拡大縮小時に正常に検出ができなくなる。
* 16bit grayscale 画像から自前で チャンネル分解してシェーダー内で統合する。
	* 詳しくは DevidePngViewModel.cs のコメント参照。
 
 */

namespace GmlConverter.Views.UserControls
{
	/// <summary>
	/// CreateMask.xaml の相互作用ロジック
	/// </summary>
	public partial class CreateMask : UserControl
	{
		private ScrollScaleImageController _previewController;
		private CreateMaskViewModel _vm;

		public CreateMask()
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
		
		private void MaskModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (previewImage == null)
				return;

			if (e.RemovedItems.Count > 0)
			{
				var unselected = (EffectType)maskMode.Items.IndexOf(e.RemovedItems[0]);
				switch (unselected)
				{
				case EffectType.HillShade:
					hillShadeSettings.Visibility = Visibility.Collapsed;
					break;
				case EffectType.AmbientOcclusion:
					ambientOcclusionSettings.Visibility = Visibility.Collapsed;
					break;
				case EffectType.Curvature:
					curvatureSettings.Visibility = Visibility.Collapsed;
					break;
				}
			}

			var selected = (EffectType)maskMode.SelectedIndex;
			switch (selected)
			{
			case EffectType.Height:
				previewImage.Effect = Resources["HeightShaderEffect"] as Effect;
				break;
			case EffectType.HillShade:
				previewImage.Effect = Resources["HillShadeShaderEffect"] as Effect;
				hillShadeSettings.Visibility = Visibility.Visible;
				break;
			case EffectType.AmbientOcclusion:
				previewImage.Effect = Resources["AmbientOcclusionShaderEffect"] as Effect;
				ambientOcclusionSettings.Visibility = Visibility.Visible;
				break;
			case EffectType.Curvature:
				previewImage.Effect = Resources["CurvatureShaderEffect"] as Effect;
				curvatureSettings.Visibility = Visibility.Visible;
				break;
			default:
				previewImage.Effect = null;
				break;
			}

			_vm.MaskModeSelectedIndex = selected;
		}
	}

	public class EffectTypeStringDictionary
	{
		// ComboBoxの一覧に表示するデータ
		public Dictionary<EffectType, string> EffectTypeNameDictionary { get; } = new();

		public EffectTypeStringDictionary()
		{
			var values = Enum.GetValues(typeof(EffectType)) as EffectType[];
			if (values == null)
				return;

			foreach (var value in values)
				EffectTypeNameDictionary.Add(value, value.ToString());
		}
	}
}
