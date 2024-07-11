using GmlConverter.Utilities;
using ImageMagick;
using OpenCvSharp;
using System.CodeDom;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using static OpenCvSharp.FileStorage;

namespace GmlConverter.ViewModels
{
	internal class NoDataToSlopeViewModel : ViewModelBase, IDisposable
	{
		#region BindingProperties

		public bool IsSaveButtonEnabled
		{
			get => !Processing && _image != null;
		}

		#region HillShade
		private double _hillShadeLightSourceAzimuth = 315.0;
		public double HillShadeLightSourceAzimuth
		{
			get => _hillShadeLightSourceAzimuth;
			set
			{
				if (_hillShadeLightSourceAzimuth != value)
				{
					_hillShadeLightSourceAzimuth = value;
					OnPropertyChanged();
				}
			}
		}
		private double _hillShadeLightSourceAltitude = 45.0;
		public double HillShadeLightSourceAltitude
		{
			get => _hillShadeLightSourceAltitude;
			set
			{
				if (_hillShadeLightSourceAltitude != value)
				{
					_hillShadeLightSourceAltitude = value;
					OnPropertyChanged();
				}
			}
		}
		private bool _hillShadeDrawShadeIsChecked = true;
		public bool HillShadeDrawShadeIsChecked
		{
			get => _hillShadeDrawShadeIsChecked;
			set
			{
				if (_hillShadeDrawShadeIsChecked != value)
				{
					_hillShadeDrawShadeIsChecked = value;
					//OnPropertyChanged(); // コードから更新されず、参照するコントロールもないので不要。
					OnPropertyChanged(nameof(HillShadeDrawShade));
				}
			}
		}
		public double HillShadeDrawShade
		{
			get => _hillShadeDrawShadeIsChecked ? 1 : 0;
		}
		#endregion


		private double _pixelDistance = 10;
		public double PixelDistance
		{
			get => _pixelDistance;
			private set
			{
				if (_pixelDistance != value)
				{
					_pixelDistance = value;
					OnPropertyChanged();
				}
			}
		}

		private double _imageWidth = 2048;
		public double ImageWidth
		{
			get => _imageWidth;
			private set
			{
				if (_imageWidth != value)
				{
					_imageWidth = value;
					OnPropertyChanged();
				}
			}
		}

		private double _imageHeight = 2048;
		public double ImageHeight
		{
			get => _imageHeight;
			private set
			{
				if (_imageHeight != value)
				{
					_imageHeight = value;
					OnPropertyChanged();
				}
			}
		}

		private string _inputFileNameLabel = string.Empty;
		public string InputFileNameLabel
		{
			get => _inputFileNameLabel;
			private set
			{
				if (_inputFileNameLabel != value)
				{
					_inputFileNameLabel = value;
					OnPropertyChanged();
				}
			}
		}


		#endregion

		#region PrivateProperties

		private BitmapSource? _previewBitmapSource = null;
		public BitmapSource? PreviewBitmapSource
		{
			get => _previewBitmapSource;
			private set
			{
				if (_previewBitmapSource != value)
				{
					_previewBitmapSource = value;
					OnPropertyChanged();
				}
			}
		}
		//private Brush? _previewBrush = null;
		//public Brush? PreviewBrush
		//{
		//	get => _previewBrush;
		//	private set
		//	{
		//		if (_previewBrush != value)
		//		{
		//			_previewBrush = value;
		//			OnPropertyChanged();
		//		}
		//	}
		//}

		private MagickImage? _image = null;
		private int _imagePixelDistance = 0;

		private FileNameHolder _fileNameHolder = new();

		private AngleMap _angleMap = new();
		private DistanceMap _distanceMap = new();

		#endregion

		internal NoDataToSlopeViewModel()
		{
			ProcessingChanged += () =>
			{
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
		}

		~NoDataToSlopeViewModel() => Dispose();

		public void Dispose()
		{
			_image?.Dispose();
			_image = null;
			_distanceMap.Dispose();
		}


		internal void Load()
		{
			var filePath = SelectFileToLoad();
			if (filePath == null)
				return;

			FileNameHolder fileNameHolder = new FileNameHolder(filePath);
			// Update output path
			if (fileNameHolder.DirectoryName != string.Empty)
				_svm.IOPath = fileNameHolder.DirectoryName;

			var inputImagePixelDistance = fileNameHolder.PixelDistance;
			if (inputImagePixelDistance == 0)
				return;

			// 現在の内容をクリア
			PreviewBitmapSource = null;
			//PreviewBrush = null;
			_image?.Dispose();
			_image = null;

			InputFileNameLabel = fileNameHolder.FileName;
			UpdateStatusLabel($"Loading {fileNameHolder.FileName}.");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Run(() =>
			{
				try
				{
					//filePath から MagickImage を生成する。
					//Save するまで保持するので using は使わない。
					MagickImage inputImage = new(fileNameHolder.FilePath, MagickFormat.Png00);

					//デカくてダメそうならここでリサイズする。

					_image = inputImage;
					_imagePixelDistance = inputImagePixelDistance;
					_fileNameHolder = fileNameHolder;
					_angleMap.Update(_image, _imagePixelDistance);
					_distanceMap.Update(_image, _angleMap, SlopeDepthScale, SlopeDistanceScale, SlopeInitialDepth);

					//フォーマットを変換する。
					//16bit grayscale を r g チャンネルに分解し、シェーダーで扱えるようにする。
					//var outputFileName = $"{_fileNameHolder.DirectoryName}\\Preview.png";
					//var previewBitmapSource = BitmapSourceHelper.ToBitmapSourceFrom16bitGrayscaleToBgr(_image);
					var previewBitmapSource = BitmapSourceHelper.ToBitmapSourceFrom16bitGrayscaleToBgr(_distanceMap.Image ?? _image);

					previewBitmapSource.Freeze();

					Application.Current.Dispatcher.Invoke(new(() =>
					{
						PixelDistance = inputImagePixelDistance;
						ImageWidth = previewBitmapSource.PixelWidth;
						ImageHeight = previewBitmapSource.PixelHeight;
						PreviewBitmapSource = previewBitmapSource;
						//PreviewBrush = new ImageBrush(PreviewBitmapSource);
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
		internal string? SelectFileToLoad()
		{
			// Configure open file dialog box
			Microsoft.Win32.OpenFileDialog dialog = new()
			{
				AddExtension = true,
				CheckFileExists = true,
				DefaultExt = ".png",
				Filter = "Heightmap image (.png)|*.png",
				InitialDirectory = _svm.IOPath,
				Multiselect = false,
				Title = "Select png file to load.",
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open file dialog box results
			if (!result)
			{
				return null;
			}
			return dialog.FileName;
		}

		internal void Save()
		{
			if (_image == null)
				return;

			var filePath = SelectFileToSave();
			if (filePath == null)
				return;

			FileNameHolder fileNameHolder = new FileNameHolder(filePath);
			if (fileNameHolder.DirectoryName != string.Empty)
				_svm.IOPath = fileNameHolder.DirectoryName;

			UpdateStatusLabel($"Saveing {fileNameHolder.FileName}.");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Run(() =>
			{
				try
				{
				//ここでファイルの保存
				//_distanceMap.Image?.Write(filePath);
					if (_distanceMap.Image != null)
						PngWriter.WritePng(filePath, _distanceMap.Image);
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

		internal string? SelectFileToSave()
		{
			// Configure open file dialog box
			Microsoft.Win32.SaveFileDialog dialog = new()
			{
				AddExtension = true,
				DefaultExt = ".png",
				FileName = _fileNameHolder.CreateRelatedFileName("SlopeNoData"),
				Filter = "PNG Filess|*.png",
				InitialDirectory = _svm.IOPath,
				OverwritePrompt = true,
				Title = "Select a file to save the png file."
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open file dialog box results
			if (!result)
			{
				return null;
			}
			return dialog.FileName;
		}


		private double _slopeDepthScale = 100;
		public double SlopeDepthScale
		{
			get => _slopeDepthScale;
			set
			{
				if(_slopeDepthScale != value)
				{
					_slopeDepthScale = value;
					OnPropertyChanged(nameof(SlopeDepthScale));
					OnPropertyChanged(nameof(IsSlopeApplyButtonEnabled));
				}
			}
		}
		private double _slopeDistanceScale = 100;
		public double SlopeDistanceScale
		{
			get => _slopeDistanceScale;
			set
			{
				if (_slopeDistanceScale != value)
				{
					_slopeDistanceScale = value;
					OnPropertyChanged(nameof(SlopeDistanceScale));
					OnPropertyChanged(nameof(IsSlopeApplyButtonEnabled));
				}
			}
		}
		private double _slopeInitialDepth = 0;
		public double SlopeInitialDepth
		{
			get => _slopeInitialDepth;
			set
			{
				if (_slopeInitialDepth != value)
				{
					_slopeInitialDepth = value;
					OnPropertyChanged(nameof(SlopeInitialDepth));
					OnPropertyChanged(nameof(IsSlopeApplyButtonEnabled));
				}
			}
		}
		
		public bool IsSlopeApplyButtonEnabled
		{
			get => _distanceMap.IsChangedSlopeSettings(SlopeDepthScale, SlopeDistanceScale, SlopeInitialDepth);
		}

		internal void SlopeApply()
		{
			if(_image == null)
				return;
			UpdateStatusLabel($"Update Slope Settings...");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Run(() =>
			{
				try
				{
					//_distanceMap.Update をすると _angleMap が汚れるので、更新し直す。
					_angleMap.Update(_image, _imagePixelDistance);
					_distanceMap.Update(_image, _angleMap, SlopeDepthScale, SlopeDistanceScale, SlopeInitialDepth);

					//フォーマットを変換する。
					//16bit grayscale を r g チャンネルに分解し、シェーダーで扱えるようにする。
					//var outputFileName = $"{_fileNameHolder.DirectoryName}\\Preview.png";
					//var previewBitmapSource = BitmapSourceHelper.ToBitmapSourceFrom16bitGrayscaleToBgr(inputImage);
					var previewBitmapSource = BitmapSourceHelper.ToBitmapSourceFrom16bitGrayscaleToBgr(_distanceMap.Image ?? _image);

					previewBitmapSource.Freeze();

					Application.Current.Dispatcher.Invoke(new(() =>
					{
						PreviewBitmapSource = previewBitmapSource;
						//PreviewBrush = new ImageBrush(PreviewBitmapSource);
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
						OnPropertyChanged(nameof(IsSlopeApplyButtonEnabled));
					}));
				}

			}, cancellationToken); _distanceMap.Update(_image, _angleMap, SlopeDepthScale, SlopeDistanceScale, SlopeInitialDepth);
		}
	}
}
