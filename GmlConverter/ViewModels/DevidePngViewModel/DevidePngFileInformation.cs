using GmlConverter.Utilities;
using ImageMagick;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// DevidePngFileInformation Class
	/// Requires using System.ComponentModel;
	/// </summary>
	internal class DevidePngFileInformation : INotifyPropertyChanged, IDisposable /*, IComparable*/ /*, IEditableObject */
	{
		// The GmlFileInformation class implements INotifyPropertyChanged and IEditableObject
		// so that the datagrid can properly respond to changes to the
		// data collection and edits made in the DataGrid.

		/// <summary>
		/// Implement INotifyPropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		// The calling member's name will be used as the parameter.
		protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

		private FileNameHolder _fileNameHolder;

		public string FilePath { get => _fileNameHolder.FilePath; }
		public string DirectoryName { get => _fileNameHolder.DirectoryName; }
		public string FileName { get => _fileNameHolder.FileName; }
		public string FileNameWithoutExtension { get => _fileNameHolder.FileNameWithoutExtension; }
		public string Extension { get => _fileNameHolder.Extension; }

		public System.Drawing.Size Size { get; private set; }

		private MagickImage? _magickImage;

		private BitmapSource? _bitmapSource;
		public BitmapSource? BitmapSource
		{
			get =>_bitmapSource;
			private set => _bitmapSource = value; 
		}

		internal DevidePngFileInformation(string filePath)
		{
			_fileNameHolder = new(filePath);
			_magickImage = null;
			_bitmapSource = null;
		}

		~DevidePngFileInformation() => Dispose();

		public void Dispose()
		{
			_magickImage?.Dispose();
			_magickImage = null;
		}

		internal bool CheckSize(System.Drawing.Size checkSize)
		{
			MagickImageInfo info = new(FilePath);
			System.Drawing.Size size = new(info.Width, info.Height);
			return (checkSize == size);
		}

		internal void Read()
		{
			_magickImage = new(FilePath);
			Size = new(_magickImage.Width, _magickImage.Height);
			_bitmapSource = _magickImage.ToBitmapSource();
			_bitmapSource.Freeze();
		}

		internal int ClampZeroDivCeil(int a, int b)
		{
			//int rem = 0;
			//var div = Math.DivRem(a, b, out rem);
			//return div + (rem != 0 ? 1 : 0);
			return (a <= 0)? 0: ((a + b - 1) / b);
		}

		internal int CountGrid(int range, int point, int spacing)
		{
			var halfl = spacing / 2;
			var halfr = halfl + (((spacing & 1) != 0) ? 1 : 0);

			var l = ClampZeroDivCeil(point - halfl, spacing);
			var r = ClampZeroDivCeil(range - (point + halfr), spacing);
			return l + r + 1;
		}
		internal int GetOriginal(int point, int spacing)
		{
			var halfl = spacing / 2;
			//var l = point - halfl;
			//if(l < 0)
			//{
			//	return -l;
			//}
			//else
			//{
			//	var rem = l % spacing;
			//	return rem == 0 ? 0 : (spacing - rem);
			//}
			var l = point + spacing - halfl;
			var rem = l % spacing;
			return rem == 0 ? 0 : (spacing - rem);
		}

		internal void Write(string fileDirectory, System.Drawing.Point centerPoint, int gridSpacing, bool isIncludeBoundaryLines, CancellationToken cancellationToken )
		{
			if (_magickImage == null)
				return;

			using var image = _magickImage.Clone();
			if (image == null)
				return;

			var toCorner = new System.Drawing.Size(gridSpacing / 2, gridSpacing / 2);
			var cornerPoint = centerPoint + toCorner;

			var rows = CountGrid(image.Height, centerPoint.Y, gridSpacing);
			var cols = CountGrid(image.Width, centerPoint.X, gridSpacing);

			var newWidth = cols * gridSpacing;
			var newHeight = rows * gridSpacing;

			var originX = GetOriginal(centerPoint.X, gridSpacing);
			var originY = GetOriginal(centerPoint.Y, gridSpacing);

			image.BackgroundColor = MagickColors.Black;
			image.Extent(-originX, -originY, newWidth, newHeight);
			image.Format = MagickFormat.Png00;

			var outputWH = isIncludeBoundaryLines ? gridSpacing + 1 : gridSpacing;

			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < cols; x++)
				{
					using (var i = image.Clone(new MagickGeometry(x * gridSpacing, y * gridSpacing, outputWH, outputWH)) as MagickImage)
					{
						if (i == null)
							continue;
						//i.Format = MagickFormat.Png00;
						//i.Write($"{fileDirectory}\\{FileNameWithoutExtension}_spacing{gridSpacing}_x{x}_y{y}.png");
						PngWriter.WritePng($"{fileDirectory}\\{FileNameWithoutExtension}_spacing{gridSpacing}_x{x}_y{y}.png", i);
						cancellationToken.ThrowIfCancellationRequested();
					}
				}
			}
		}
	}
}

