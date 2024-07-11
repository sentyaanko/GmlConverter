using GmlConverter.Models.Gml;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// Png の描画に関わる情報
	/// </summary>
	internal class PngDrawInformation
	{
		internal System.Drawing.Point Point { get; set; }
		internal double Scale { get; set; }

		internal PngDrawInformation(System.Drawing.Point point, double scale)
		{
			Point = point;
			Scale = scale;
		}

		/// <summary>
		/// ファクトリ関数
		/// </summary>
		/// <param name="pngInformation"></param>
		/// <param name="pngInformationMinMax"></param>
		/// <param name="pngSizeInformation"></param>
		/// <returns></returns>
		internal static PngDrawInformation Create(PngInformation pngInformation, PngInformationMinMax pngInformationMinMax, PngSizeInformation pngSizeInformation)
		{
			var mesh2Size = pngSizeInformation.Mesh2Size;
			var areaSize = pngSizeInformation.AreaSize;
			var outputPixelDistance = pngInformationMinMax.PixelDistance.Min;
			var myMesh3Size = GmlHelpers.GetMesh3Size(pngInformation.PixelDistance);
			return new
			(
				new
				(
					//スケールに関する計算がいるはず
					pngInformation.Left.SubMesh2(pngInformationMinMax.Left.Min) * mesh2Size.Width + pngInformation.Left.Mesh3 * myMesh3Size.Width,
					(areaSize.Height - 1 - pngInformation.Top.SubMesh2(pngInformationMinMax.Top.Min)) * mesh2Size.Height + (10 - 1 - pngInformation.Top.Mesh3) * myMesh3Size.Height
				),
				(double)pngInformation.PixelDistance / outputPixelDistance
			);
		}

	}
}
