using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// ViewModel の基底クラス
	/// </summary>
	internal class ViewModelBase : INotifyPropertyChanged
	{
		/// <summary>
		/// Implement INotifyPropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		// The calling member's name will be used as the parameter.
		protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

		#region Processing
		/// <summary>
		/// 処理中か
		/// </summary>
		public bool Processing
		{
			get => _svm.Processing;
			protected set
			{
				if (_svm.Processing != value)
				{
					_svm.Processing = value;
					//OnPropertyChanged();	// 自身は Binding されないので不要。
					OnPropertyChanged(nameof(IsNotProcessing));
					OnProcessingChanged();
					
				}
			}
		}
		public bool IsNotProcessing
		{
			get => !Processing;
		}

		#endregion

		#region StatusLabel
		public string StatusLabel
		{
			get => _svm.StatusLabel;
			protected set
			{
				if (_svm.StatusLabel != value)
				{
					_svm.StatusLabel = value;
				}
			}
		}
		#endregion


		/// <summary>
		/// 共有 ViewModel
		/// </summary>
		protected SharedViewModel _svm;

		/// <summary>
		/// OnMainWindowButtonEnabledChanged() から呼ばれるアクション
		/// </summary>
		internal event Action? ProcessingChanged;

		/// <summary>
		/// MainWindow のボタンの有効状態が切り替わった際に呼ばれるイベント
		/// </summary>
		protected virtual void OnProcessingChanged()
		{
			ProcessingChanged?.Invoke();
		}



		protected ViewModelBase()
		{
			var app = Application.Current as App;
			var serviceProvider = app?.ServiceProvider;
			_svm = serviceProvider?.GetRequiredService<SharedViewModel>() ?? new();
		}


		protected void UpdateStatusLabel(string statusLabel)
		{
			StatusLabel = statusLabel;
			Processing = StatusLabel != string.Empty;
		}
	}
}
