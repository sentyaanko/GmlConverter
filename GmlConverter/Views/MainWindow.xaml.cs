using GmlConverter.Views.UserControls;
using GmlConverter.ViewModels;
using System.Windows;

namespace GmlConverter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel _vm;

		public MainWindow()
		{
			_vm = new();
			DataContext = _vm;
			InitializeComponent();
			stepContainer.Content = new ConvertGmlToPng();
		}

		private void ConvertGmlToPngButton_Click(object sender, RoutedEventArgs e) => ChangeStepContainer<ConvertGmlToPng>();
		private void TilePngButton_Click(object sender, RoutedEventArgs e) => ChangeStepContainer<TilePng>();
		private void CreateMaskButton_Click(object sender, RoutedEventArgs e) => ChangeStepContainer<CreateMask>();
		private void NoDataToSlopeButton_Click(object sender, RoutedEventArgs e) => ChangeStepContainer<NoDataToSlope>();
		private void DevidePngButton_Click(object sender, RoutedEventArgs e) => ChangeStepContainer<DevidePng>();

		/// <summary>
		/// StepContainer の切り替え処理。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private void ChangeStepContainer<T>()where T : new()
		{
			if (stepContainer.Content != null)
			{
				if (stepContainer.Content is T)
					return;
				var disposable = stepContainer.Content as IDisposable;
				disposable?.Dispose();
			}
			stepContainer.Content = new T();
		}
	}
}
	