using GmlConverter.Utilities;
using ImageMagick;
using OpenCvSharp;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

/*

削除済みの過去のコードに関するコメント。

# LinearGray について

* [https://imagemagick.org/script/command-line-options.php#colorspace]
	```LinearGray = 0.298839 * R + 0.586811 * G + 0.114350 * B```
* LinearGray にするということは上記の変換がかけられるということ。つまり当然色が変わる。
* また、 8bit に変換される


# 16bit PNG を開く際の指定

* 何も指定せずに開き、そのまま使うのが最善。
	* なので、 settings を渡して開くこともできるが、意味がない。
		```csharp
			var fmt = new MagickReadSettings() 
			{
				//ColorSpace = ColorSpace.LinearGray,
				ColorType = ColorType.Grayscale, 
				Format = MagickFormat.Png, Depth = 16 
			};
			MagickImage inputImage = new(filePath, fmt);
		```
	* こういう書き方はできない。(no pixels defined in cache `' @ error/cache.c/OpenPixelCache/3690)
		```csharp
			MagickImage inputImage = new();
			inputImage.ColorSpace = ColorSpace.LinearGray;	//そもそも LinearGray は用途が違う。
			inputImage.ColorType = ColorType.Grayscale;
			inputImage.Format = MagickFormat.Gray;			//Png が正しい。（Gray を指定しても意味ないはず）
			inputImage.Depth = 16;
			inputImage.Read(filePath, MagickFormat.Png);
		```
* そのまま開けば下記の出力が得られる。
	```csharp
		Debug.WriteLine($"input images ColorSpace is {inputImage.ColorSpace}.");		// Gray
		Debug.WriteLine($"input images ColorType is {inputImage.ColorType}.");			// Grayscale
		Debug.WriteLine($"input images Gamma is {inputImage.Gamma}.");					// 0.45454545454545453
		Debug.WriteLine($"input images Depthis {inputImage.Depth}.");					// 16
		Debug.WriteLine($"input images ChannelCount is {inputImage.ChannelCount}.");    // 1
	```
* Gamma は設定されているけど、 Grayscale の PNG はそういうものらしい。


# 16 bit PNG を G/B チャンネルに分けた BitmapSource を作る方法について

* 案 1
	* 概要
		```
			16bit 0b fedc ba98 7654 3210
			を
				r 0b eca8 6420
				g 0b fdb9 7531
			に分解して、シェーダー内で統合する方法。
			これにより、単純に r/g チャンネルに値を分けることで、 g チャンネルの値が ff -> 0 に急激に値が変わることを避け、
			サンプラーがおかしなブレンドされた値を取得してこないようにする。
		```
	* 結果
		```
			ps_3_0 ではビット演算は使用できないため、シェーダー内で値を復元できない。
			たかだか256種類なので、シェーダー内でテーブル持ってもいいのだが…。
		```
* 案 2
	* 概要
		```
			16bit grayscale をそのままシェーダーに渡す方法
			Gray16 は ToBitmapSource() では作れない。自前で作る必要がある。
		```
	* 結果
		```
			試したところ、どうにも変なエッジが発生する。
			よくわからないのでプログラムで出力した内容を表示してみたところ、
			下位バイトの情報が欠落する。
			（何故か上位バイトと同じ値になってしまう。）
			（16bit の上位バイトが上位下位両方に設定された状態で計算された値が取得される）
			 例：0x3c31 の値が、シェーダー内で取得すると 0x3c3c 相当の値(0x3c3c / 65535.0)に変わっている
		```
* 案 3
	* 概要
		```
			Evaluate を使い、16bit Grayscale を g:上位 b:下位 の 8bit grayscale に変換し、 MagickImage.Combine() でチャンネル統合する。
		```
	* 結果
		```
			工程のどこか(Conbine() もしくは ToBitmapSource() )でガンマ値の参照を行っているのか、色がぶっ壊れる。
			例： 0x6049 => 0x1007 など
		```

# 16 bit PNG を保存する方法について

* MagickImage の Fx を利用した変換。
	* ピクセル毎に処理するため、再計算が多く遅い。使用しない。
* 普通の変換。
	* Intrinsics 大差ないが、向こうを使うのでこちらは使わない。


 */


namespace GmlConverter.ViewModels
{
	/// <summary>
	/// ViewModel for CreateMask  User Contril
	/// </summary>
	internal class CreateMaskViewModel : ViewModelBase, IDisposable
	{
		#region BindingProperties

		public bool IsSaveButtonEnabled
		{
			get => !Processing && _image != null && (_maskModeSelectedIndex != EffectType.Height && _maskModeSelectedIndex != EffectType.HillShade);
		}

		private EffectType _maskModeSelectedIndex = EffectType.Height;
		public EffectType MaskModeSelectedIndex
		{
			get => _maskModeSelectedIndex;
			set
			{
				if (_maskModeSelectedIndex != value)
				{
					_maskModeSelectedIndex = value;
					OnPropertyChanged(nameof(IsSaveButtonEnabled));
				}
			}
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

		#region AmbientOcclusion
		private double _ambientOcclusionAdjustSliderValue = 1.0;
		public double AmbientOcclusionAdjustSliderValue
		{
			get => _ambientOcclusionAdjustSliderValue;
			set
			{
				if (_ambientOcclusionAdjustSliderValue != value)
				{
					_ambientOcclusionAdjustSliderValue = value;
					OnPropertyChanged();
				}
			}
		}

		private double _ambientOcclusionExponentSliderValue = 1.0;
		public double AmbientOcclusionExponentSliderValue
		{
			get => _ambientOcclusionExponentSliderValue;
			set
			{
				if (_ambientOcclusionExponentSliderValue != value)
				{
					_ambientOcclusionExponentSliderValue = value;
					OnPropertyChanged();
				}
			}
		}
		#endregion

		#region Curvature
		private double _curvatureAdjustSliderValue = 50.0;
		public double CurvatureAdjustSliderValue
		{
			get => _curvatureAdjustSliderValue;
			set
			{
				if (_curvatureAdjustSliderValue != value)
				{
					_curvatureAdjustSliderValue = value;
					OnPropertyChanged();
				}
			}
		}

		private double _curvatureExponentSliderValue= 1.0;
		public double CurvatureExponentSliderValue
		{
			get => _curvatureExponentSliderValue;
			set
			{
				if (_curvatureExponentSliderValue != value)
				{
					_curvatureExponentSliderValue = value;
					OnPropertyChanged();
				}
			}
		}
		
		private double _curvatureAngleBaseSliderValue = 0.0;
		public double CurvatureAngleBaseSliderValue
		{
			get => _curvatureAngleBaseSliderValue;
			set
			{
				if (_curvatureAngleBaseSliderValue != value)
				{
					_curvatureAngleBaseSliderValue = value;
					OnPropertyChanged();
				}
			}
		}

		private double _curvatureMaskHeightMinSliderValue = -200;
		public double CurvatureMaskHeightMinSliderValue
		{
			get => _curvatureMaskHeightMinSliderValue;
			set
			{
				if (_curvatureMaskHeightMinSliderValue != value)
				{
					_curvatureMaskHeightMinSliderValue = value;
					OnPropertyChanged();
				}
			}
		}

		private double _curvatureMaskHeightMaxSliderValue = 4000;
		public double CurvatureMaskHeightMaxSliderValue
		{
			get => _curvatureMaskHeightMaxSliderValue;
			set
			{
				if (_curvatureMaskHeightMaxSliderValue != value)
				{
					_curvatureMaskHeightMaxSliderValue = value;
					OnPropertyChanged();
				}
			}
		}

		private bool _curvatureIsPositive = true;
		public bool CurvatureIsPositive
		{
			get => _curvatureIsPositive;
			set
			{
				if (_curvatureIsPositive != value)
				{
					_curvatureIsPositive = value;
					OnPropertyChanged(nameof(CurvaturePositive));
				}
			}
		}
		public double CurvaturePositive
		{
			get => (_curvatureIsPositive) ? 1.0 : -1.0;
		}
		public double CurvatureAngleDirection
		{
			get => _curvatureIsAngleDirection? -1: 1;
		}

		private static double[][] s_CurvatureAngleDirections =
		[
			[ +0.0, -1.0, +0.0, +0.0 ],
			[ +0.0, -1.0, -1.0, +0.0 ],
			[ +0.0, +0.0, -1.0, +0.0 ],
			[ +0.0, +0.0, -1.0, +1.0 ],
			[ +0.0, +0.0, +0.0, +1.0 ],
			[ +1.0, +0.0, +0.0, +1.0 ],
			[ +1.0, +0.0, +0.0, +0.0 ],
			[ +1.0, +1.0, +0.0, +0.0 ],
			[ +0.0, +1.0, +0.0, +0.0 ],
			[ +0.0, +1.0, +1.0, +0.0 ],
			[ +0.0, +0.0, +1.0, +0.0 ],
			[ +0.0, +0.0, +1.0, -1.0 ],
			[ +0.0, +0.0, +0.0, -1.0 ],
			[ -1.0, +0.0, +0.0, -1.0 ],
			[ -1.0, +0.0, +0.0, +0.0 ],
			[ -1.0, -1.0, +0.0, +0.0 ],
		];
		public double CurvatureAngleDirection0
		{
			get => (_curvatureIsAngleDirection) ? s_CurvatureAngleDirections[(int)_curvatureAngleDirectionSliderValue][0] : 1;
		}
		public double CurvatureAngleDirection1
		{
			get => (_curvatureIsAngleDirection) ? s_CurvatureAngleDirections[(int)_curvatureAngleDirectionSliderValue][1] : 1;
		}
		public double CurvatureAngleDirection2
		{
			get => (_curvatureIsAngleDirection) ? s_CurvatureAngleDirections[(int)_curvatureAngleDirectionSliderValue][2] : 1;
		}
		public double CurvatureAngleDirection3
		{
			get => (_curvatureIsAngleDirection) ? s_CurvatureAngleDirections[(int)_curvatureAngleDirectionSliderValue][3] : 1;
		}

		public bool _curvatureIsAngleDirection = false;
		public bool CurvatureIsAngleDirection
		{
			get => _curvatureIsAngleDirection;
			set
			{
				if (_curvatureIsAngleDirection != value)
				{
					_curvatureIsAngleDirection = value;
					//OnPropertyChanged(); //コードから更新されないので不要
					OnPropertyChanged(nameof(CurvatureAngleDirection));
					OnPropertyChanged(nameof(CurvatureAngleDirection0));
					OnPropertyChanged(nameof(CurvatureAngleDirection1));
					OnPropertyChanged(nameof(CurvatureAngleDirection2));
					OnPropertyChanged(nameof(CurvatureAngleDirection3));
				}
			}
		}
		//private int _curvatureAngleDirectionMask = 0;
		private double _curvatureAngleDirectionSliderValue;
		public double CurvatureAngleDirectionSliderValue
		{
			get => _curvatureAngleDirectionSliderValue;
			set
			{
				if (_curvatureAngleDirectionSliderValue != value)
				{
					_curvatureAngleDirectionSliderValue = value;
					OnPropertyChanged(); //Label からも参照されるので必要
					OnPropertyChanged(nameof(CurvatureAngleDirection0));
					OnPropertyChanged(nameof(CurvatureAngleDirection1));
					OnPropertyChanged(nameof(CurvatureAngleDirection2));
					OnPropertyChanged(nameof(CurvatureAngleDirection3));
				}
			}
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

		#endregion

		internal CreateMaskViewModel()
		{
			ProcessingChanged += () =>
			{
				OnPropertyChanged(nameof(IsSaveButtonEnabled));
			};
		}

		~CreateMaskViewModel() => Dispose();

		public void Dispose()
		{
			_image?.Dispose();
			_image = null;
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

					MagickImage inputImage = new(fileNameHolder.FilePath);

					//デカくてダメそうならここでリサイズする。

					_image = inputImage;
					_imagePixelDistance = inputImagePixelDistance;
					_fileNameHolder = fileNameHolder;
					_angleMap.Update(_image, _imagePixelDistance);

					//フォーマットを変換する。
					//16bit grayscale を r g チャンネルに分解し、シェーダーで扱えるようにする。
					var previewBitmapSource = BitmapSourceHelper.ToBitmapSourceFrom16bitGrayscaleToBgr(inputImage);

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
					switch (MaskModeSelectedIndex)
					{
					case EffectType.AmbientOcclusion:
						WriteAmbientOcclusion_Intrinsics(fileNameHolder.FilePath);
						break;
					case EffectType.Curvature:
						WriteCurvature_Intrinsics(fileNameHolder.FilePath);
						break;
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

		internal string? SelectFileToSave()
		{
			var curvature = CurvatureIsAngleDirection ? "Angle" : "Curvature";
			var negaposi = CurvatureIsPositive ? "Positive" : "Negative";
			var modeStr = MaskModeSelectedIndex switch
			{
				EffectType.AmbientOcclusion => "AmbientOcclusion",
				EffectType.Curvature => $"{curvature}{negaposi}",
				_ => "unknown"
			};

			// Configure open file dialog box
			Microsoft.Win32.SaveFileDialog dialog = new()
			{
				AddExtension = true,
				DefaultExt = ".png",
				FileName = _fileNameHolder.CreateRelatedFileName(modeStr),
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

		internal void WriteAmbientOcclusion_Intrinsics(string filePath)
		{
			if (_image == null)
				return;
			//フォーマットを変換する。
			//AngleMap からつくる。
			using var pixels = _image.GetPixelsUnsafe();
			var pixcelDataGray16 = pixels.ToArray();
			if (pixcelDataGray16 == null)
			{
				throw new Exception();
			}

			var atanMap = _angleMap.Angles;
			if (atanMap == null)
			{
				throw new Exception();
			}
			var height = _angleMap.Height;
			var width = _angleMap.Width;

			//ここでイメージを作る。
			var ad = Math.Round(AmbientOcclusionAdjustSliderValue, 2);
			var ex = Math.Round(AmbientOcclusionExponentSliderValue, 2);
			//var hs = 0.1; // p の値が[0-65535]。カラー値 1 毎に 0.1 [m] なので 0.1 を乗算する。
			//Vector256<float> と乗算するので MathF のまま。
			var co = 2 / MathF.PI;
			var invex = 1 / ex;

			//ushort[] buf = new ushort[width * height];
			byte[] buf = new byte[width * height * 2];
			int index = 0;
			int indexAtanMap = 0;
			int indexPixcelDataGray16 = 0;
			for (int iY = 0; iY < height; ++iY)
			{
				for (int iX = 0; iX < width; ++iX)
				{
					var gray = pixcelDataGray16[indexPixcelDataGray16++];
					if (gray == 0)
					{
						indexAtanMap += 8;
						buf[index++] = 0;
						buf[index++] = 0;
					}
					else
					{
						var angle = Vector256.Create(atanMap, indexAtanMap); indexAtanMap += 8;
						var aos = Vector256<float>.One - Vector256.Max(Vector256<float>.Zero, Vector256.Min(Vector256<float>.One, angle * co));
						var aoav = (double)Vector256.Dot(aos, Vector256<float>.One) / 8;
						var ao = Math.Max(0, Math.Min( 1, Math.Pow(aoav * ad, invex)));
						var v = (ushort)((gray == 0) ? 0 : (65535 * ao));

						buf[index++] = (byte)(0xff & v);
						buf[index++] = (byte)(0xff & (v >> 8));
					}
				}
			}
			//using (var ambientOcclusionImage = new MagickImage(MagickColors.Black, width, height))
			//{
			//	ambientOcclusionImage.ColorSpace = ColorSpace.Gray;
			//	ambientOcclusionImage.ColorType = ColorType.Grayscale;
			//	ambientOcclusionImage.Depth = 16;
			//	using var pixelsOut = ambientOcclusionImage.GetPixelsUnsafe();
			//	pixelsOut.SetPixels(buf);

			//	ambientOcclusionImage.Write(filePath);
			//}
			PngWriter.WritePng(filePath, width, height, buf);
		}

		internal void WriteCurvature_Intrinsics(string filePath)
		{
			if (_image == null)
				return;
			//フォーマットを変換する。
			//AngleMap からつくる。
			using var pixels = _image.GetPixelsUnsafe();
			var pixcelDataGray16 = pixels.ToArray();
			if (pixcelDataGray16 == null)
			{
				throw new Exception();
			}

			var atanMap = _angleMap.Angles;
			if (atanMap == null)
			{
				throw new Exception();
			}
			var height = _angleMap.Height;
			var width = _angleMap.Width;

			//ここでイメージを作る。
			var db = PixelDistance;
			var dh = db * Math.Sqrt(2);
			var ad = Math.Round(CurvatureAdjustSliderValue, 1);
			var hl = Math.Round(CurvatureMaskHeightMinSliderValue, 1);
			var hh = Math.Round(CurvatureMaskHeightMaxSliderValue, 1);
			var ex = Math.Round(CurvatureExponentSliderValue, 1);
			var ab = Math.Round(CurvatureAngleBaseSliderValue, 1);
			var hs = 0.1; // p の値が[0-65535]。カラー値 1 毎に 0.1 [m] なので 0.1 を乗算する。
			//var co = ad / 4 / Math.PI; //4項目の相加平均算出のための/4
			var co = ad / Math.PI; //項目数が変動するようになったのでここでは /4 しない
			var ip = CurvatureIsPositive ? 1 : -1;

			//ushort[] buf = new ushort[width * height];
			byte[] buf = new byte[width * height * 2];
			//Stopwatch sw = Stopwatch.StartNew();
			int index = 0;
			int indexAtanMap = 0;
			int indexPixcelDataGray16 = 0;
			var invdb = (float)(1 / db);
			var invdh = (float)(1 / dh);
			var invex = 1 / ex;
			var angleBase = Vector128.Create((float)ab);
			var angleDirection = Vector128.Create((float)CurvatureAngleDirection0, (float)CurvatureAngleDirection1, (float)CurvatureAngleDirection2, (float)CurvatureAngleDirection3);
			var angleLen = Vector128.Dot(angleDirection, angleDirection);
			var invds = Vector128.Create(invdh, invdb, invdh, invdb) * angleDirection;
			var fCurvatureAngleDirection = (float)CurvatureAngleDirection;
			co /= angleLen;	//ここで相加平均用の除算をする
			for (int iY = 0; iY < height; ++iY)
			{
				for (int iX = 0; iX < width; ++iX)
				{
					var gray = pixcelDataGray16[indexPixcelDataGray16++];
					if (gray == 0)
					{
						indexAtanMap += 8;
						buf[index++] = 0;
						buf[index++] = 0;
					}
					else
					{
						var angle0123 = Vector128.Create(atanMap, indexAtanMap); indexAtanMap += 4;
						var angle8765 = Vector128.Create(atanMap, indexAtanMap); indexAtanMap += 4;
						var add = angle0123 + angle8765 * fCurvatureAngleDirection + angleBase;
						var cu = (double)Vector128.Dot(add, invds);

						var hx = (gray == 0) ? 0 : (gray * hs - 500);
						var cus = ((hl < hx) && (hx < hh)) ? Math.Max(0, Math.Min(1, Math.Pow(Math.Abs(cu * co), invex))) : 0;
						var v = (ushort)((ip * cu < 0) ? 0 : (65535 * cus));
						buf[index++] = (byte)(0xff & v);
						buf[index++] = (byte)(0xff & (v>>8));
					}
				}
			}
			//sw.Stop();
			//Debug.WriteLine($"WriteCurvature_Intrinsics{sw.ToString()}");
			//using (var outputImage = new MagickImage(MagickColors.Black, width, height))
			//{
			//	outputImage.ColorSpace = ColorSpace.Gray;
			//	outputImage.Settings.SetDefine("png:bit-depth", "16");
			//	outputImage.Format = MagickFormat.Png00;
			//	using var pixelsOut = outputImage.GetPixelsUnsafe();
			//	pixelsOut.SetPixels(buf);
			//	outputImage.Write(filePath);
			//}
			PngWriter.WritePng(filePath, width, height, buf);
		}
	}
}
