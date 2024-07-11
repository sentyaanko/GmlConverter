using GmlConverter.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GmlConverter.ViewModels
{
	internal class DevidePngViewModel : ViewModelBase, IDisposable
	{
		#region BindingProperties
		public bool IsSaveButtonEnabled
		{
			get => !Processing && _fileInformations.Count != 0 && IsCenterInsideBitmap();
		}

		private ObservableCollection<DevidePngFileInformation> _fileInformations = new ();
		public ObservableCollection<DevidePngFileInformation> FileInformations
		{
			get => _fileInformations;
		}

		private bool _isOpenRelatedFiles = true;
		public bool IsOpenRelatedFiles
		{
			get => _isOpenRelatedFiles;
			set
			{
				if (_isOpenRelatedFiles != value)
				{
					_isOpenRelatedFiles = value;
					//OnPropertyChanged(); コードから更新されないので不要
				}
			}
		}

		private int _gridSpacingIndex = 0;
		public int GridSpacingIndex
		{
			get => _gridSpacingIndex;
			set
			{
				if (_gridSpacingIndex != value)
				{
					_gridSpacingIndex = value;
					//OnPropertyChanged(); //コードから変更しないので不要。
					OnPropertyChanged(nameof(GridSpacing));
				}
			}
		}
		public int GridSpacing
		{
			get
			{
				var result = GridSpacingIndex switch
				{
					0 => 127,
					1 => 253,
					2 => 505,
					3 => 1009,
					4 => 2017,
					5 => 4033,
					6 => 8129,
					_ => 0,
				};
				return (result == 0) ? 0 : IsIncludeBoundaryLines ? result - 1 : result;
			}
		}

		private bool _isIncludeBoundaryLines = false;
		public bool IsIncludeBoundaryLines
		{
			get => _isIncludeBoundaryLines;
			set
			{
				if (_isIncludeBoundaryLines != value)
				{
					_isIncludeBoundaryLines = value;
					//OnPropertyChanged(); //コードから変更しないので不要。
					OnPropertyChanged(nameof(GridSpacing));
				}
			}
		}
		


		private System.Drawing.Point _centerPoint = new (-1);
		public System.Drawing.Point CenterPoint
		{
			get => _centerPoint;
			set
			{
				if (_centerPoint != value)
				{
					_centerPoint = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(IsSaveButtonEnabled));
				}
			}
		}

		public double PreviewWidth
		{
			get => PreviewBitmapSource?.PixelWidth?? 0;
		}
		public double PreviewHeight
		{
			get => PreviewBitmapSource?.PixelHeight ?? 0;
		}

		public Visibility GridCanvasVisibility
		{
			get => (PreviewBitmapSource != null) ? Visibility.Visible : Visibility.Hidden;
		}

		private DevidePngFileInformation? _fileInformationSelectedItem = null;
		public DevidePngFileInformation? FileInformationSelectedItem
		{
			get => _fileInformationSelectedItem;
			set
			{
				if (_fileInformationSelectedItem != value)
				{
					_fileInformationSelectedItem = value;
					//OnPropertyChanged(); コードから更新されないので不要
					OnPropertyChanged(nameof(PreviewBitmapSource));
					OnPropertyChanged(nameof(PreviewWidth));
					OnPropertyChanged(nameof(PreviewHeight));
					OnPropertyChanged(nameof(GridCanvasVisibility));
					OnPropertyChanged(nameof(IsSaveButtonEnabled));
				}
			}
		}


		public BitmapSource? PreviewBitmapSource
		{
			get => _fileInformationSelectedItem?.BitmapSource;
		}
		#endregion

		#region PrivateProperties
		#endregion

		internal DevidePngViewModel()
		{
			_fileInformations.CollectionChanged += (s, e) =>
			{
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
			ProcessingChanged += () =>
			{
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
		}

		~DevidePngViewModel() => Dispose();

		public void Dispose()
		{
			if(FileInformations.Count > 0)
			{
				foreach (var item in FileInformations)
				{
					item.Dispose();
				}
				FileInformations.Clear();
			}
		}

		internal void Load()
		{
			// Configure open file dialog box
			Microsoft.Win32.OpenFileDialog dialog = new()
			{
				AddExtension = true,
				CheckFileExists = true,
				DefaultExt = ".png",
				Filter = "Png files. (.png)|*.png",
				InitialDirectory = _svm.IOPath,
				Multiselect = true,
				Title = "Select png files to load.",
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open file dialog box results
			if (!result)
			{
				return;
			}

			UpdateStatusLabel($"Loading Png files.");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			var isEmpty = FileInformations.Count == 0;
			var currentSize = !isEmpty? FileInformations[0].Size: new();
			var containFilePaths = FileInformations.Select(item => item.FilePath).ToList();

			Task.Run(() =>
			{
				try
				{
					var addPngFiles = AddPngFiles(containFilePaths, dialog.FileNames, IsOpenRelatedFiles, cancellationToken);
					cancellationToken.ThrowIfCancellationRequested();
					if (addPngFiles.Count > 0)
					{
						List<DevidePngFileInformation> toAdd = new();
						//一つも読んでいない場合は最初のを読む
						if (isEmpty)
						{
							var addPngFile = addPngFiles[0];
							addPngFile.Read();
							toAdd.Add(addPngFile);
							addPngFiles.RemoveAt(0);
							currentSize = addPngFile.Size;
						}
						cancellationToken.ThrowIfCancellationRequested();

						//2個目以降は png を開いてサイズを確認して、異なったら無視するようにする。
						foreach (var addPngFile in addPngFiles)
						{
							if (addPngFile.CheckSize(currentSize))
							{
								addPngFile.Read();
								toAdd.Add(addPngFile);
							}
							cancellationToken.ThrowIfCancellationRequested();
						}
						Application.Current.Dispatcher.Invoke(new(() =>
						{
							toAdd.ForEach(item => FileInformations.Add(item));
						}));
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
		internal List<DevidePngFileInformation> AddPngFiles(List<string> containFilePaths, string[] filePaths, bool isOpenRelatedFiles, CancellationToken cancellationToken)
		{
			List<string> tempFilePaths = new();
			if (isOpenRelatedFiles)
			{
				foreach (var filePath in filePaths)
				{
					tempFilePaths.Add(filePath);
					FileNameHolder fileNameHolder = new(filePath);
					var relatedFilePaths = System.IO.Directory.GetFiles(
						fileNameHolder.DirectoryName,
						fileNameHolder.GetRelatedFilesWildCard()).ToList();
					if(relatedFilePaths != null)
					{
						tempFilePaths.AddRange(relatedFilePaths);
					}
					cancellationToken.ThrowIfCancellationRequested();
				}
			}
			else
			{
				tempFilePaths.AddRange(filePaths);
			}
			cancellationToken.ThrowIfCancellationRequested();
			List<DevidePngFileInformation> result = new();
			foreach (var filePath in tempFilePaths.Distinct().Where(item => !containFilePaths.Contains(item)))
			{
				result.Add(new(filePath));
				cancellationToken.ThrowIfCancellationRequested();
			}
			return result;
		}
		internal void Save()
		{
			// Configure open folder dialog box
			Microsoft.Win32.OpenFolderDialog dialog = new()
			{
				InitialDirectory = _svm.IOPath,
				//InitialDirectory = System.IO.Directory.GetCurrentDirectory(),
				Multiselect = false,
				Title = "Select a folder to save the png files.",
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open folder dialog box results
			if (!result)
			{
				return;
			}
			var outputPath = dialog.FolderName;
			_svm.IOPath = outputPath;

			UpdateStatusLabel($"Saveing divide png.");

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			Task.Run(() =>
			{
				try
				{
					foreach (var fileInformationin in FileInformations)
					{
						fileInformationin.Write(outputPath, CenterPoint, GridSpacing, IsIncludeBoundaryLines, cancellationToken);
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


		internal void RemoveRows(List<DevidePngFileInformation> removeList)
		{
			removeList.ForEach(item => FileInformations.Remove(item));
		}

		internal void SetCenterPoint(System.Drawing.Point point)
		{
			CenterPoint = point;
		}
		private bool IsCenterInsideBitmap()
		{
			if(PreviewBitmapSource == null)
				return false;
			System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, PreviewBitmapSource.PixelWidth, PreviewBitmapSource.PixelHeight);
			return rect.Contains(CenterPoint);
		}
	}
}

