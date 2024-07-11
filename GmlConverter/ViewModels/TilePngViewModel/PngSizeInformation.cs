using GmlConverter.Models.Gml;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// Png の描画に関わる情報
	/// </summary>
	internal class PngSizeInformation
	{
		/// <summary>
		/// 結合時に使う Mesh2 毎の大きさ
		/// </summary>
		internal System.Drawing.Size Mesh2Size;

		/// <summary>
		/// 結合時に使う Mesh2 の枚数
		/// </summary>
		internal System.Drawing.Size AreaSize;

		/// <summary>
		/// 結合時に使う画像の大きさ、すなわちキャンバスサイズ
		/// </summary>
		internal System.Drawing.Size ImageSize;
		internal PngSizeInformation(PngInformationMinMax pngInformationMinMax)
		{
			//Debug.WriteLine($"Output PixelDistance: {pngInformationMinMax.PixelDistance.Min}");
			//Debug.WriteLine($"Min left,bottom: {pngInformationMinMax.Left.Min},{pngInformationMinMax.Bottom.Min}");
			//Debug.WriteLine($"Max right,top: {pngInformationMinMax.Right.Max}, {pngInformationMinMax.Top.Max}");

			//最小で作る。出力時のPixel距離が異なる場合は後でリサイズするので、ここでは何もしない。
			Mesh2Size = GmlHelpers.GetMesh2Size(pngInformationMinMax.PixelDistance.Min);
			AreaSize = pngInformationMinMax.GetMesh2AreaSize();
			ImageSize = new(AreaSize.Width * Mesh2Size.Width, AreaSize.Height * Mesh2Size.Height);

			//Debug.WriteLine($"number of mesh W,H: {AreaSize.Width}, {AreaSize.Height}");
			//Debug.WriteLine($"output image W,H: {ImageSize.Width}, {ImageSize.Height}");
		}
	}
}
