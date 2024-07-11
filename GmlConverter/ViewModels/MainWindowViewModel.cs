namespace GmlConverter.ViewModels
{
	/// <summary>
	/// ViewModel for MainWindow
	/// </summary>
	internal class MainWindowViewModel : ViewModelBase
	{
		internal MainWindowViewModel()
		{
			_svm.ProcessingChanged += () =>
			{
				OnPropertyChanged(nameof(IsNotProcessing));
			};
			_svm.StatusLabelChanged += () =>
			{
				OnPropertyChanged(nameof(StatusLabel));
			};
			
		}
	}
}
