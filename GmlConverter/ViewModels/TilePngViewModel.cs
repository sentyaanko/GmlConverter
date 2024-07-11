using GmlConverter.Utilities;
using ImageMagick;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// ViewModel for TilePng  User Contril
	/// </summary>
	internal class TilePngViewModel : ViewModelBase, IDisposable
	{
		#region BindingProperties
		public bool IsSaveButtonEnabled
		{
			get => !Processing && _image != null && _processingFileNames.Count == 0;
		}

		private System.Drawing.Size _inputSize = new();
		public System.Drawing.Size InputSize
		{
			get => _inputSize;
			private set
			{
				if (_inputSize != value)
				{
					_inputSize = value;
					OnPropertyChanged();
				}
			}
		}
		private int _inputPixelDistance = 0;
		public int InputPixelDistance
		{
			get => _inputPixelDistance;
			private set
			{
				if (_inputPixelDistance != value)
				{
					_inputPixelDistance = value;
					OnPropertyChanged();
				}
			}
		}
		private System.Drawing.Size _outputSize = new();
		public System.Drawing.Size OutputSize
		{
			get => _outputSize;
			private set
			{
				if (_outputSize != value)
				{
					_outputSize = value;
					OnPropertyChanged();
				}
			}
		}
		private int _outputPixelDistance = 0;
		public int OutputPixelDistance
		{
			get => _outputPixelDistance;
			private set
			{
				if (_outputPixelDistance != value)
				{
					_outputPixelDistance = value;
					OnPropertyChanged();
				}
			}
		}

		private int _pixelDistanceIndex = 0;
		public int PixelDistanceIndex
		{
			get => _pixelDistanceIndex;
			set
			{
				if (_pixelDistanceIndex != value)
				{
					_pixelDistanceIndex = value;
					OnPropertyChanged();
					UpdateOutputInformation();
				}
			}
		}

		private bool _isTrimming = false;
		public bool IsTrimming
		{
			get => _isTrimming;
			set
			{
				if (_isTrimming != value)
				{
					_isTrimming = value;
					// OnPropertyChanged(); //コードから変更しないので不要。
				}
			}
		}

		private bool _isMargin = false;
		public bool IsMargin
		{
			get => _isMargin;
			set
			{
				if(_isMargin != value)
				{
					_isMargin = value;
					// OnPropertyChanged(); //コードから変更しないので不要。
				}
			}
		}

		private double _marginSize = 0;
		public double MarginSize
		{
			get => _marginSize;
			set
			{
				if(_marginSize != value)
				{
					_marginSize = value;
					OnPropertyChanged();
				}
			}
		}

		private SectionSize _marginSectionSize = SectionSize.invalid;
		public SectionSize MarginSectionSize
		{
			get => _marginSectionSize;
			set
			{
				if (_marginSectionSize != value)
				{
					_marginSectionSize = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// プレビュー用
		/// </summary>
		private BitmapSource? _previewImageSource = null;
		public BitmapSource? PreviewBitmapSource
		{
			get => _previewImageSource;
			private set
			{
				if (_previewImageSource != value)
				{
					_previewImageSource = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region PrivateProperties
		/// <summary>
		/// 処理中のファイル名一覧
		/// </summary>
		private List<string> _processingFileNames = new();

		/// <summary>
		/// 保存用
		/// </summary>
		private MagickImage? _image = null;

		#endregion

		internal TilePngViewModel()
		{
			ProcessingChanged += () =>
			{
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
		}

		~TilePngViewModel() => Dispose();

		public void Dispose()
		{
			_image?.Dispose();
			_image = null;
		}

		internal void Load()
		{
			var FolderToLoad = SelectFolderToLoad();
			if (FolderToLoad == null)
				return;

			_svm.IOPath = FolderToLoad;

			// 現在の内容をクリア
			PreviewBitmapSource = null;
			_image?.Dispose();
			_image = null;

			InputSize = new();
			InputPixelDistance = 0;
			OutputSize = new();
			OutputPixelDistance = 0;
			UpdateStatusLabel("Get TargetFiles.");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Run(() =>
			{
				try
				{
					var filePaths = System.IO.Directory.GetFiles(FolderToLoad, "????-??-????-*.png");

					if (filePaths.Length == 0)
						return;
					PngInformationMinMax? pngInformationMinMax = null;

					List<PngInformation> pngInformations = new();

					foreach (var filePath in filePaths)
					{
						if (filePath == null)
							continue;
						FileNameHolder fileNameHolder = new(filePath);

						var pattern = @"([0-9]{2})([0-9]{2})-([0-7])([0-7])-([0-9])([0-9])([0-9])([0-9])-(1|5|10)\.png";
						var v = PngInformation.Create(fileNameHolder, pattern);
						if (v == null)
							continue;

						pngInformations.Add(v);
						//Debug.WriteLine($"Matched value: {v.FilePath}");
						//Debug.WriteLine($"1st mesh:{v.Left.Mesh1}, {v.Bottom.Mesh1}");
						//Debug.WriteLine($"2nd mesh:{v.Left.Mesh2}, {v.Bottom.Mesh2}");
						//Debug.WriteLine($"PixelDistance: {v.PixelDistance}");

						Application.Current.Dispatcher.Invoke(new(() =>
						{
							_processingFileNames.Add(fileNameHolder.FilePath);
						}));

						if (pngInformationMinMax == null)
							pngInformationMinMax = new(v);
						else
							pngInformationMinMax.Update(v);
					}
					if (pngInformationMinMax == null)
						return;

					Func<int, string> UodateLabel = c => c switch
					{
						0 => string.Empty,
						1 => $"Loading {c} file.",
						_ => $"Loading {c} files."
					};
					Application.Current.Dispatcher.Invoke(new(() =>
					{
						UpdateStatusLabel(UodateLabel(_processingFileNames.Count));
					}));

					PngSizeInformation pngSizeInformation = new(pngInformationMinMax);

					//描画座標設定
					pngInformations.ForEach(item => item.UpdateDrawInformation(pngInformationMinMax, pngSizeInformation));

					//Save するまで保持するので using は使わない。
					MagickImage outputImage = new(MagickColors.Black, pngSizeInformation.ImageSize.Width, pngSizeInformation.ImageSize.Height) { ColorSpace = ColorSpace.LinearGray, Depth = 16 };
					//Debug.WriteLine($"output: {outputImage.ColorSpace}, {outputImage.Depth}");
					pngInformations.ForEach(item =>
					{
						//Debug.WriteLine("proc: {item.FileName}");
						//Debug.WriteLine("Load.");
						using (MagickImage InputImage = new(item.FilePath))
						{
							var pngDrawInformation = item.PngDrawInformation;
							if (pngDrawInformation != null)
							{
								var scale = pngDrawInformation.Scale;
								//Debug.WriteLine("Load finished.");
								//Debug.WriteLine($"{item.FileName}: {InputImage.ColorSpace}, {InputImage.Depth}");
								if (scale != 1)
								{
									//Debug.WriteLine("Scale.");
									InputImage.Resize((int)(InputImage.Width * scale), (int)(InputImage.Height * scale));
									//Debug.WriteLine("Scale finished.");
								}
								//Debug.WriteLine("Composite.");
								outputImage.Composite(InputImage, pngDrawInformation.Point.X, pngDrawInformation.Point.Y, CompositeOperator.Over);
								//Debug.WriteLine("Composite finished.");
							}
						}
						Application.Current.Dispatcher.Invoke(new(() =>
						{
							_processingFileNames.Remove(item.FilePath);
							UpdateStatusLabel(UodateLabel(_processingFileNames.Count));
						}));

						cancellationToken.ThrowIfCancellationRequested();
					});

					Application.Current.Dispatcher.Invoke(new(() =>
					{
						InputSize = new(outputImage.Width, outputImage.Height);
						InputPixelDistance = pngInformationMinMax.PixelDistance.Min;
						UpdateOutputInformation();

						_image = outputImage;
						UpdateStatusLabel("ToBitmapSource...");
					}));

					var previewBitmapSource = outputImage.ToBitmapSource();
					previewBitmapSource.Freeze();

					Application.Current.Dispatcher.Invoke(new(() =>
					{
						PreviewBitmapSource = previewBitmapSource;
					}));
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
				}
				finally
				{
					Application.Current.Dispatcher.Invoke(new(() =>
					{
						UpdateStatusLabel(string.Empty);
					}));
				}
			}, cancellationToken);
		}

		/// <summary>
		/// InputPixelDistance / InputSize / PixelDistanceIndex を元に
		/// OutputPixelDistance / OutputSize を更新する。
		/// </summary>
		internal void UpdateOutputInformation()
		{
			var pixelDistanceSetting = PixelDistance();
			var pixelDistanceOutput = pixelDistanceSetting > 0 ? pixelDistanceSetting : InputPixelDistance;
			if (pixelDistanceOutput == 0)
				return;

			OutputPixelDistance = pixelDistanceOutput;
			OutputSize = (InputPixelDistance != OutputPixelDistance) ? (InputSize * InputPixelDistance / OutputPixelDistance) : InputSize;
		}

		private string? SelectFolderToLoad()
		{
			// Configure open folder dialog box
			Microsoft.Win32.OpenFolderDialog dialog = new()
			{
				InitialDirectory = _svm.IOPath,
				Multiselect = false,
				Title = "Select a folder to load the png files."
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open folder dialog box results
			if (!result)
			{
				return null;
			}
			return dialog.FolderName;

		}

		internal void Save()
		{
			if (_image == null)
				return;

			var filePathToSave = SelectFileToSave();
			if (filePathToSave == null)
				return;

			FileNameHolder fileNameHolder = new FileNameHolder(filePathToSave);
			// Update output path
			if (fileNameHolder.DirectoryName != string.Empty)
				_svm.IOPath = fileNameHolder.DirectoryName;

			UpdateStatusLabel($"Save to {fileNameHolder.FileName}.");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			var inputPixelDistance = InputPixelDistance;
			var outputPixelDistance = OutputPixelDistance;

			Task.Run(() =>
			{
				try
				{
					using (var outputImage = _image.Clone())
					{
						//ここでリサイズ
						if (inputPixelDistance != outputPixelDistance)
						{
							Application.Current.Dispatcher.Invoke(new(() =>
							{
								UpdateStatusLabel(StatusLabel + "Resize...");
							}));
							outputImage.Resize(new Percentage(100.0 * inputPixelDistance / outputPixelDistance));
						}
						if(IsTrimming)
						{
							Application.Current.Dispatcher.Invoke(new(() =>
							{
								UpdateStatusLabel(StatusLabel + "Trim...");
							}));
							outputImage.Trim();
							outputImage.RePage();
						}
						if(IsMargin && (MarginSize != 0 || MarginSectionSize != SectionSize.invalid))
						{
							Application.Current.Dispatcher.Invoke(new(() =>
							{
								UpdateStatusLabel(StatusLabel + "Margin...");
							}));
							var margin2 = (int)MarginSize * 2;
							var width = outputImage.Width + margin2;
							var height = outputImage.Height + margin2;
							var sectionSize = (int)MarginSectionSize;
							if(sectionSize != 0)
							{
								Func<int, int, int> f = (length, section) =>
								{
									var (quotient, remainder) = Math.DivRem(length, section);
									if (remainder == 1)
										return length;
									return length + section - remainder + 1;
								};
								width = f(width, sectionSize);
								height = f(height, sectionSize);
							}
							if(width != outputImage.Width || height != outputImage.Height)
							{
								outputImage.BackgroundColor = MagickColors.Black;
								outputImage.Extent(width, height, Gravity.Center);
							}
						}

						//outputImage.Write(fileNameHolder.FilePath);
						var mi = outputImage as MagickImage;
						if (mi != null)
							PngWriter.WritePng(fileNameHolder.FilePath, mi);
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
				}
				finally
				{
					Application.Current.Dispatcher.Invoke(new(() =>
					{
						UpdateStatusLabel(string.Empty);
					}));
				}
			}, cancellationToken);
		}

		private string? SelectFileToSave()
		{
			// Configure save filePath dialog box
			Microsoft.Win32.SaveFileDialog dialog = new()
			{
				AddExtension = true,
				DefaultExt = ".png",
				FileName = $"heightmap-{OutputPixelDistance}.png",
				Filter = "PNG Filess|*.png",
				InitialDirectory = _svm.IOPath,
				OverwritePrompt = true,
				Title = "Select a file to save the png file."
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open folder dialog box results
			if (!result)
			{
				return null;
			}
			return dialog.FileName;
		}

		private int PixelDistance() => PixelDistanceIndex switch { 0 => -1, 1 => 1, 2 => 5, 3 => 10, _ => -1 };
	}
}
