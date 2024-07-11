using GmlConverter.Utilities;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// Png を並べる際の最小値と最大値関連の情報
	/// </summary>
	internal class PngInformationMinMax
	{
		internal MinMax<MeshLocationUnit> Left;
		internal MinMax<MeshLocationUnit> Bottom;
		internal MinMax<MeshLocationUnit> Right;
		internal MinMax<MeshLocationUnit> Top;
		internal MinMax<int> PixelDistance;

		internal PngInformationMinMax(PngInformation pngInformation)
		{
			Left = new(pngInformation.Left);
			Bottom = new(pngInformation.Bottom);
			Right = new(pngInformation.Right);
			Top = new(pngInformation.Top);
			PixelDistance = new(pngInformation.PixelDistance);
		}
		internal void Update(PngInformation pngInformation)
		{
			Left.Update(pngInformation.Left);
			Bottom.Update(pngInformation.Bottom);
			Right.Update(pngInformation.Right);
			Top.Update(pngInformation.Top);
			PixelDistance.Update(pngInformation.PixelDistance);
		}

		/// <summary>
		/// すべての Png を内包するサイズの取得
		/// </summary>
		/// <returns></returns>
		internal System.Drawing.Size GetMesh2AreaSize()
			=> new(Right.Max.SubMesh2(Left.Min) + 1, Top.Max.SubMesh2(Bottom.Min) + 1);
	}
}
