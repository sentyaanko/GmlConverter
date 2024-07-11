using GmlConverter.Models.Gml;
using GmlConverter.Utilities;
using ImageMagick;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GmlConverter.ViewModels
{
    /// <summary>
    /// ViewModel for ConvertGmlToPng User Contril
    /// </summary>
    internal class ConvertGmlToPngViewModel : ViewModelBase
	{
		#region BindingProperties

		public bool IsSaveButtonEnabled
		{
			get => !Processing && _gmlFileInformations.Count != 0;
		}

		private ObservableCollection<GmlFileInformation> _gmlFileInformations = new();
		public ObservableCollection<GmlFileInformation> GmlFileInformations
		{
			get => _gmlFileInformations;
		}

		private GmlFileInformation? _gmlFileInformationSelectedItem = null;
		public GmlFileInformation? GmlFileInformationSelectedItem
		{
			get => _gmlFileInformationSelectedItem;
			set
			{
				if (_gmlFileInformationSelectedItem != value)
				{
					_gmlFileInformationSelectedItem = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(PreviewBitmapSource));
					OnPropertyChanged(nameof(PixelDistance));
					OnPropertyChanged(nameof(ImageWidth));
					OnPropertyChanged(nameof(ImageHeight));
				}
			}
		}
		public BitmapSource? PreviewBitmapSource
		{
			get => _gmlFileInformationSelectedItem?.HeightmapBitmapSource;
		}

		public double PixelDistance
		{
			get => _gmlFileInformationSelectedItem?.GmlHeader.GridDistance ?? 0;
		}

		public double ImageWidth
		{
			get => _gmlFileInformationSelectedItem?.HeightmapBitmapSource.Width ?? 0;
		}

		public double ImageHeight
		{
			get => _gmlFileInformationSelectedItem?.HeightmapBitmapSource.Height ?? 0;
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
					OnPropertyChanged(); // Label や Effect からも参照されるので必要。
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
					OnPropertyChanged(); // Label や Effect からも参照されるので必要。
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
			get => _hillShadeDrawShadeIsChecked? 1: 0;
		}

		#endregion

		#endregion

		#region PrivateProperties

		private List<string> _processingFileNames = new();

		private bool _isSaving = false;

		#endregion

		internal ConvertGmlToPngViewModel()
		{
			_gmlFileInformations.CollectionChanged += (s, e) =>
			{
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
			ProcessingChanged += () =>
			{ 
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
		}

		internal void AddGmlFiles(string[] files)
		{
			//出力中は受け付けない
			if (_isSaving)
			{
				return;
			}
			if (files.Length == 0)
			{
				return;
			}
			var addFiles = files.ToList();

			//読み込み済みのファイルを除外する
			if (GmlFileInformations.Count != 0)
			{
				var ignore = GmlFileInformations.Select(item => item.GmlHeader.FileName).ToList();
				addFiles = addFiles.Except(ignore).ToList();
				if (addFiles.Count == 0)
				{
					return;
				}
			}
			//読込中のファイルを除外する
			if (_processingFileNames.Count != 0)
			{
				addFiles = addFiles.Except(_processingFileNames).ToList();
				if (addFiles.Count == 0)
				{
					return;
				}
			}
			foreach (var it in addFiles)
				_processingFileNames.Add(it);
			Func<int, string> UodateLabel = c => c switch
			{
				0 => string.Empty,
				1 => $"Loading {c} file.",
				_ => $"Loading {c} files."
			};
			UpdateStatusLabel(UodateLabel(_processingFileNames.Count));

			// ProgressDialogViewModel の派生クラス側でゴニョゴニョする方法を検討する。
			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			List<Task> tasks = new();
			foreach (string addFile in addFiles)
			{
				tasks.Add(Task.Run(() =>
				{
					try
					{
						var result = GmlFileInformation.Create(addFile, cancellationToken);
						if (result != null)
						{
							Application.Current.Dispatcher.Invoke(new(() =>
							{
								GmlFileInformations.Add(result);
							}));
						}
					}
					catch (OperationCanceledException e)
					{
						Debug.WriteLine($"canceled:{e.Message}");
					}
					finally
					{
						Application.Current.Dispatcher.Invoke(new(() =>
						{
							_processingFileNames.Remove(addFile);
							UpdateStatusLabel(UodateLabel(_processingFileNames.Count));
						}));

					}

				}, cancellationToken));
			}
			//var taskWhenAll = Task.WhenAll(tasks);
		}

		internal void Load()
		{
			// Configure open file dialog box
			Microsoft.Win32.OpenFileDialog dialog = new()
			{
				AddExtension = true,
				CheckFileExists = true,
				DefaultExt = ".xml",
				Filter = "Gml documents (.xml)|*.xml",
				InitialDirectory = _svm.IOPath,
				Multiselect = true,
				Title = "Select gml files to load.",
			};

			// Show open folder dialog box
			var result = dialog.ShowDialog() ?? false;

			// Process open file dialog box results
			if (!result)
			{
				return;
			}
			AddGmlFiles(dialog.FileNames);
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
			_svm.IOPath = dialog.FolderName;

			SaveMeshSecondHeightmap(_svm.IOPath, CreateMeshNumber2SecondMeshContainer());
		}

		internal void RemoveRows(List<GmlFileInformation> removeList)
		{
			removeList.ForEach(item => GmlFileInformations.Remove(item));
		}

		/// <summary>
		/// mesh2 が同じ領域の Gml の情報を集める
		/// </summary>
		/// <returns></returns>
		private Dictionary<(int, int), SecondMeshContainer> CreateMeshNumber2SecondMeshContainer()
		{
			Dictionary<(int, int), SecondMeshContainer> meshNumber2SecondMeshContainer = new();

			foreach (var gmlFileInformation in GmlFileInformations)
			{
				if (!gmlFileInformation.Active)
				{
					continue;
				}
				var key = (gmlFileInformation.GmlHeader.MeshNumber.Mesh1.Number, gmlFileInformation.GmlHeader.MeshNumber.Mesh2.Number);
				if (meshNumber2SecondMeshContainer.ContainsKey(key))
				{
					meshNumber2SecondMeshContainer[key].Add(gmlFileInformation);
				}
				else
				{
					meshNumber2SecondMeshContainer.Add(key, new(gmlFileInformation));
				}

			}
			return meshNumber2SecondMeshContainer;
		}

		/// <summary>
		/// mesh2 毎に heightmap の png を出力
		/// mesh2 全体のデータがなければ（mesh2 が無く、 mesh3 がすべて（10x10）揃っていなければ ）包含する最小矩形を出力する。
		/// </summary>
		/// <param name="fullPathToFolder">出力パス</param>
		/// <param name="meshNumber2SecondMeshContainer">mesh2 毎の Gml 情報</param>
		private void SaveMeshSecondHeightmap(string fullPathToFolder, Dictionary<(int, int), SecondMeshContainer> meshNumber2SecondMeshContainer)
		{
			if (meshNumber2SecondMeshContainer.Count == 0)
			{
				return;
			}
			List<string> outputFiles = new();
			foreach (var it in meshNumber2SecondMeshContainer)
			{
				var (mesh1, mesh2) = it.Key;
				outputFiles.Add($"{mesh1:0000}-{mesh2:00}");
			}
			foreach (var it in outputFiles)
				_processingFileNames.Add(it);
			Func<int, string> UodateLabel = c => c switch
			{
				0 => string.Empty,
				1 => $"Save {c} file.",
				_ => $"Save {c} files."
			};
			UpdateStatusLabel(UodateLabel(_processingFileNames.Count));
			_isSaving = true;

			CancellationTokenSource cancellationTokenSource = new();
			var cancellationToken = cancellationTokenSource.Token;

			foreach (var it in meshNumber2SecondMeshContainer)
			{
				Task.Run(() =>
				{
					var (mesh1, mesh2) = it.Key;
					var secondMeshName = $"{mesh1:0000}-{mesh2:00}";
					try
					{
						var rect = it.Value.Rect;
						var gridDistance = it.Value.GridDistance;
						var canvasSize = it.Value.GetCanvasSize();
						//Debug.WriteLine($"mesh:{secondMeshName}-{rect},gridDistance:{gridDistance.Min}-{gridDistance.Max},surfaceSize:{canvasSize},");

						DrawingGroup imageDrawings = new();

						//拡大時に補間をしない
						RenderOptions.SetBitmapScalingMode(imageDrawings, BitmapScalingMode.NearestNeighbor);

						foreach (var gmlFileInformation in it.Value.GmlFileInformationList.OrderByDescending(x => DemTypeExt.GetRenerPriority(x.GmlHeader.DemType)).ThenBy(x => x.GmlHeader.MeshNumber.Mesh3.Number))
						{
							var scale = gmlFileInformation.GmlHeader.GridDistance / gridDistance.Min;
							var renderPoint = gmlFileInformation.GmlHeader.MeshNumber.Mesh3.GetRenderPoint(rect);
							var divisions = gmlFileInformation.GmlHeader.GridDivisions;
							var size = divisions * scale;
							System.Drawing.Rectangle renderRect = new(new(renderPoint.X * size.Width, renderPoint.Y * size.Height), size);

							//Debug.WriteLine($"{gmlFileInformation.DisplayName},{gmlFileInformation.GmlHeader.DemTypeName},renderRect:{renderRect},scale:{scale},");

							//補間抑制の効果は見れなかったのでコメントアウト
							//RenderOptions.SetBitmapScalingMode(gmlFileInformation.Heightmap, BitmapScalingMode.NearestNeighbor);

							ImageDrawing imageDrawing = new();
							imageDrawing.Rect = new(renderRect.X, renderRect.Y, renderRect.Width, renderRect.Height);
							imageDrawing.ImageSource = gmlFileInformation.HeightmapBitmapSource;

							//補間抑制の効果は見れなかったのでコメントアウト
							//RenderOptions.SetBitmapScalingMode(imageDrawing, BitmapScalingMode.NearestNeighbor);

							imageDrawings.Children.Add(imageDrawing);

							cancellationToken.ThrowIfCancellationRequested();
						}
						if (true)
						{
							DrawingVisual drawingVisual = new();
							var drawingContext = drawingVisual.RenderOpen();
							drawingContext.DrawDrawing(imageDrawings);
							drawingContext.Close();

							RenderTargetBitmap renderTargetBitmap = new(canvasSize.Width, canvasSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
							renderTargetBitmap.Render(drawingVisual);

							//grayscale bmp の作成
							int stridePbgra = (canvasSize.Width * PixelFormats.Pbgra32.BitsPerPixel + 7) / 8;
							byte[] pixcelDataPbgra = new byte[canvasSize.Height * stridePbgra];
							renderTargetBitmap.CopyPixels(pixcelDataPbgra, stridePbgra, 0);
							var bytePerPixelPbgra = PixelFormats.Pbgra32.BitsPerPixel / 8;

							int strideGray16 = (canvasSize.Width * PixelFormats.Gray16.BitsPerPixel + 7) / 8;
							byte[] pixcelDataGray16 = new byte[canvasSize.Height * strideGray16];
							var bytePerPixelGray16 = PixelFormats.Gray16.BitsPerPixel / 8;

							for (int y = 0; y < canvasSize.Height; ++y)
							{
								int offsetPbgra = y * stridePbgra;
								int offsetGray16 = y * strideGray16;
								for (int x = 0; x < canvasSize.Width; ++x)
								{
									//var b = pixcelDataPbgra[offsetPbgra + 0];//b
									//var g = pixcelDataPbgra[offsetPbgra + 1];//g
									//var r = pixcelDataPbgra[offsetPbgra + 2];//r
									//var h = (ushort)((r << 16 | g << 8 | b) / 10);
									//pixcelDataGray16[offsetGray16 + 0] = (byte)(h & 0xff);
									//pixcelDataGray16[offsetGray16 + 1] = (byte)(h >> 8 & 0xff);
									pixcelDataGray16[offsetGray16 + 0] = pixcelDataPbgra[offsetPbgra + 0];//b
									pixcelDataGray16[offsetGray16 + 1] = pixcelDataPbgra[offsetPbgra + 1];//g
									offsetPbgra += bytePerPixelPbgra;
									offsetGray16 += bytePerPixelGray16;
								}
							}


							//// ピクセルデータから画像を生成
							//var bitmap = BitmapSource.Create(
							//	canvasSize.Width, canvasSize.Height, 96, 96,
							//		PixelFormats.Gray16,
							//		BitmapPalettes.Gray16,
							//		pixcelDataGray16, strideGray16);

							////UI スレッドで使えるように Freeze する
							//bitmap.Freeze();

							//PngBitmapEncoder encoder = new();

							////encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
							//encoder.Frames.Add(BitmapFrame.Create(bitmap));

							//using (var stream = System.IO.File.Create(System.IO.Path.Combine(fullPathToFolder, $"{secondMeshName}-{rect.Y}{rect.X}{rect.Y + rect.Height - 1}{rect.X + rect.Width - 1}-{gridDistance.Min}.png")))
							//{
							//	encoder.Save(stream);
							//}

							//MagickReadSettings mrs = new()
							//{
							//	Width = canvasSize.Width,
							//	Height = canvasSize.Height,
							//	ColorSpace = ColorSpace.LinearGray,	//TODO: 要確認。おそらく不要（というか、本来あったらまずいはずなんだが）
							//	ColorType = ColorType.Grayscale,
							//	Depth = 16,
							//	Format = MagickFormat.Gray,			//TODO:png のほうがいいのでは？要確認
							//};
							//using MagickImage mi = new(pixcelDataGray16, mrs);
							//// Magick.NET を利用して gamma を指定できるかの実験。
							//// 結果はできない。読み込む際に LinearGray を指定してもガンマ値が設定されてしまう。
							////また、読み込んだあとに ColorSpace を指定するとガンマ値は変わるが、保存時には 0.45455 に戻っている。
							////なので、ロードした際にガンマ値を無視するしか方法が無い。
							////
							////Debug.WriteLine($"colorspace is {mi.ColorSpace}.");
							////Debug.WriteLine($"gamma is {mi.Gamma}.");
							////mi.ColorSpace = ColorSpace.LinearGray;
							////Debug.WriteLine($"colorspace is {mi.ColorSpace}.");
							////Debug.WriteLine($"gamma is {mi.Gamma}.");

							//mi.Write($"{fullPathToFolder}\\{secondMeshName}-{rect.Y}{rect.X}{rect.Y + rect.Height - 1}{rect.X + rect.Width - 1}-{gridDistance.Min}.png");

							PngWriter.WritePng($"{fullPathToFolder}\\{secondMeshName}-{rect.Y}{rect.X}{rect.Y + rect.Height - 1}{rect.X + rect.Width - 1}-{gridDistance.Min}.png", canvasSize.Width, canvasSize.Height, pixcelDataGray16);
						}
					}
					catch (OperationCanceledException e)
					{
						Debug.WriteLine($"canceled:{e.Message}");
					}
					finally
					{
						Application.Current.Dispatcher.Invoke(new(() =>
						{
							_processingFileNames.Remove(secondMeshName);
							UpdateStatusLabel(UodateLabel(_processingFileNames.Count));
							if (_processingFileNames.Count == 0)
							{
								_isSaving = false;
							}
						}));
					}
				});
			}
		}
	}
}
