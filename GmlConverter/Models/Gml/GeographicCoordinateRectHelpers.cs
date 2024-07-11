//using System.Windows;

//namespace GmlConverter.Models.Gml
//{
//	/// <summary>
//	/// mesh 1,2,3 の地理座標の矩形を計算するヘルパークラス
//	/// </summary>
//	internal static class GeographicCoordinateRectHelpers
//	{
//		/// <summary>
//		/// _mesh1 の1グリッドの大きさ
//		/// _mesh1 の1グリッドは経度1度ぶん
//		/// _mesh1 の1グリッドは緯度40分(=40/60度)ぶん
//		/// </summary>
//		private static Vector s_mesh1Grid = new(1.0, 40.0 / 60.0);

//		/// <summary>
//		/// _mesh2 の1グリッドの大きさ
//		/// _mesh2 の1グリッドはmesh1を8等分したサイズ
//		/// </summary>
//		private static Vector s_mesh2Grid = s_mesh1Grid / 8.0;

//		/// <summary>
//		/// _mesh3 の1グリッドの大きさ
//		/// _mesh3 の1グリッドはmesh2を10等分したサイズ
//		/// </summary>
//		private static Vector s_mesh3Grid = s_mesh2Grid / 10.0;

//		/// <summary>
//		/// _mesh1 の 原点(左下)の緯度経度を取得
//		/// </summary>
//		/// <param name="meshNumber">メッシュ番号</param>
//		/// <returns>_mesh1 の 原点(左下)の緯度経度</returns>
//		private static Point CreateMesh1OriginPoint(MeshNumber meshNumber) =>
//			new
//			(
//				meshNumber.Mesh1.X + 100.0,     //メッシュの数字に100足すと経度になる
//				meshNumber.Mesh1.Y * 1.5        //メッシュの数字に1.5倍すると緯度になる
//			);

//		/// <summary>
//		/// _mesh2 の 原点(左下)の緯度経度のオフセットを取得
//		/// </summary>
//		/// <param name="meshNumber">メッシュ番号</param>
//		/// <returns>Mesh2_ が負の場合はゼロベクトル、そうでなければ _mesh2 の 原点(左下)の緯度経度のオフセット</returns>
//		private static Vector CreateMesh2OriginVector(MeshNumber meshNumber) =>
//			meshNumber.Mesh2.IsActive
//				? new(meshNumber.Mesh2.X * s_mesh2Grid.X, meshNumber.Mesh2.Y * s_mesh2Grid.Y)
//				: new();

//		/// <summary>
//		/// _mesh3 の 原点(左下)の緯度経度のオフセットを取得
//		/// </summary>
//		/// <param name="meshNumber">メッシュ番号</param>
//		/// <returns>Mesh3_ が負の場合はゼロベクトル、そうでなければ _mesh3 の 原点(左下)の緯度経度のオフセット</returns>
//		private static Vector CreateMesh3OriginVector(MeshNumber meshNumber) =>
//			meshNumber.Mesh3.IsActive
//				? new(meshNumber.Mesh3.X * s_mesh3Grid.X, meshNumber.Mesh3.Y * s_mesh3Grid.Y)
//				: new();

//		/// <summary>
//		/// mesh1,2,3 の地理座標矩形の構築
//		/// </summary>
//		/// <param name="meshNumber">メッシュ番号</param>
//		/// <returns>mesh1,2,3 の地理座標矩形</returns>
//		internal static (GeographicCoordinateRect, GeographicCoordinateRect, GeographicCoordinateRect) CreateGeographicCoordinateRects(MeshNumber meshNumber)
//		{
//			var mesh1Origin = CreateMesh1OriginPoint(meshNumber);
//			var mesh2Origin = mesh1Origin + CreateMesh2OriginVector(meshNumber);
//			var mesh3Origin = mesh2Origin + CreateMesh3OriginVector(meshNumber);

//			return (
//				new(mesh1Origin, s_mesh1Grid),
//				new(mesh2Origin, meshNumber.Mesh2.IsActive ? s_mesh2Grid : s_mesh1Grid),
//				new(mesh3Origin, meshNumber.Mesh3.IsActive ? s_mesh3Grid : s_mesh2Grid)
//			);
//		}
//	}
//}