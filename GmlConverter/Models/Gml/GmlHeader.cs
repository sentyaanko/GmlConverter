namespace GmlConverter.Models.Gml
{
	internal class GmlHeader
	{
		/// <summary>
		/// ファイル名
		/// </summary>
		internal string FileName;

		/// <summary>
		/// メッシュ番号
		/// </summary>
		internal MeshNumber MeshNumber;

		/// <summary>
		/// "DEM種別列挙型"
		/// </summary>
		internal string DemTypeName;

		/// <summary>
		/// "DEM種別列挙型" を示す enum
		/// </summary>
		internal DemType DemType;

		/// <summary>
		/// 格子点間の距離
		/// </summary>
		internal int GridDistance;

		/// <summary>
		/// 格子の分割数
		/// </summary>
		internal System.Drawing.Size GridDivisions;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="fileName">ファイル名</param>
		/// <param name="meshNumber">メッシュ番号</param>
		/// <param name="demTypeName">"DEM種別列挙型"</param>
		/// <param name="demType">"DEM種別列挙型" を示す enum</param>
		/// <param name="gridDistance">格子点間の距離</param>
		/// <param name="gridDivisions">格子の分割数</param>
		internal GmlHeader(string fileName, MeshNumber meshNumber, string demTypeName, DemType demType, int gridDistance, System.Drawing.Size gridDivisions)
		{
			FileName = fileName;
			MeshNumber = meshNumber;
			DemTypeName = demTypeName;
			DemType = demType;
			GridDistance = gridDistance;
			GridDivisions = gridDivisions;
		}
	}
}
