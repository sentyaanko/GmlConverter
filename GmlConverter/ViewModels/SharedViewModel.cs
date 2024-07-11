namespace GmlConverter.ViewModels
{
	/// <summary>
	/// ViewModel の共用データ保持クラス
	/// </summary>
	internal class SharedViewModel
	{
		/// <summary>
		/// GmlToPng 出力パス
		/// </summary>
		internal string IOPath { get; set; }

		#region Processing

		/// <summary>
		/// 処理中か
		/// </summary>
		private bool _processing = false;
		public bool Processing
		{
			get => _processing;
			set
			{
				if (_processing != value)
				{
					_processing = value;
					OnProcessingChanged();
				}
			}
		}

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

		#endregion

		#region StatusLabel

		public string _statusLabel = string.Empty;
		public string StatusLabel
		{
			get => _statusLabel;
			set
			{
				if (_statusLabel != value)
				{
					_statusLabel = value;
					OnStatusLabelChanged();
				}
			}
		}

		/// <summary>
		/// OnMainWindowButtonEnabledChanged() から呼ばれるアクション
		/// </summary>
		internal event Action? StatusLabelChanged;

		/// <summary>
		/// MainWindow のボタンの有効状態が切り替わった際に呼ばれるイベント
		/// </summary>
		protected virtual void OnStatusLabelChanged()
		{
			StatusLabelChanged?.Invoke();
		}

		#endregion

		public SharedViewModel()
		{
			//IOPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "work");
			var currentDirectory = System.IO.Directory.GetCurrentDirectory();
#if DEBUG
			IOPath = $"{currentDirectory}\\work";
#else
			IOPath = currentDirectory;
#endif

		}
	}
}
