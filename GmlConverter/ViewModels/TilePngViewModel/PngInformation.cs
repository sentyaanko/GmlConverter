using GmlConverter.Utilities;
using System.Text.RegularExpressions;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// Png の情報
	/// </summary>
	internal class PngInformation
	{
		private FileNameHolder _fileNameHolder;
		internal string FilePath { get => _fileNameHolder.FilePath; }
		internal string FileName { get => _fileNameHolder.FileName; }
		internal MeshLocationUnit Left { get; set; }
		internal MeshLocationUnit Bottom { get; set; }
		internal MeshLocationUnit Right { get; set; }
		internal MeshLocationUnit Top { get; set; }
		internal int PixelDistance { get; set; }
		internal PngDrawInformation? PngDrawInformation { get; set; } = null;

		internal PngInformation(FileNameHolder fileNameHolder, MeshLocationUnit left, MeshLocationUnit bottom, MeshLocationUnit right, MeshLocationUnit top, int pixelDistance)
		{
			_fileNameHolder = fileNameHolder;
			Left = left;
			Bottom = bottom;
			Right = right;
			Top = top;
			PixelDistance = pixelDistance;
		}

		internal static PngInformation? Create(FileNameHolder fileNameHolder, string pattern)
		{
			var match = Regex.Match(fileNameHolder.FileName, pattern);
			if (!match.Success)
				return null;
			var mesh1Y = int.Parse(match.Groups[1].Value);
			var mesh1X = int.Parse(match.Groups[2].Value);
			var mesh2Y = int.Parse(match.Groups[3].Value);
			var mesh2X = int.Parse(match.Groups[4].Value);
			var mesh3B = int.Parse(match.Groups[5].Value);
			var mesh3L = int.Parse(match.Groups[6].Value);
			var mesh3T = int.Parse(match.Groups[7].Value);
			var mesh3R = int.Parse(match.Groups[8].Value);
			return new(
				fileNameHolder,
				new(mesh1X, mesh2X, mesh3L),
				new(mesh1Y, mesh2Y, mesh3B),
				new(mesh1X, mesh2X, mesh3R),
				new(mesh1Y, mesh2Y, mesh3T),
				int.Parse(match.Groups[9].Value)
			);
		}

		/// <summary>
		/// PngDrawInformation の更新
		/// </summary>
		/// <param name="pngInformationMinMax"></param>
		/// <param name="mesh2Size">mesh2 のピクセル数</param>
		/// <param name="areaSize">mesh2 の個数</param>
		/// <param name="outputPixelDistance"></param>
		internal void UpdateDrawInformation(PngInformationMinMax pngInformationMinMax, PngSizeInformation pngSizeInformation)
		{
			PngDrawInformation = PngDrawInformation.Create(this, pngInformationMinMax, pngSizeInformation);
			//Debug.WriteLine($"name,X,Y,scale: {FileName}, {PngDrawInformation.Point.X}, {PngDrawInformation.Point.Y}, {PngDrawInformation.Scale}");
		}

	}
}
