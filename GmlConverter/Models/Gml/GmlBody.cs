using System.Windows.Media.Imaging;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// Gml のデータ本体(標高情報の2次元配列)
	/// </summary>
	internal class GmlBody
	{
		/// <summary>
		/// 格子点から格子点の情報を取得するための辞書。
		/// </summary>
		private Dictionary<System.Drawing.Point, GridPointInfo> _point2GridPointInfo = new();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="demConfigurationPointTypeIdAndHeights">Gml ファイルの tupleList に設定されている格子点に関する情報</param>
		/// <param name="startPoint">データの開始位置</param>
		/// <param name="gridDivisions">格子の分割数</param>
		/// <param name="gridDistance">格子点間の距離</param>
		/// <returns></returns>
		internal GmlBody((DemConfigurationPointType, double)[] demConfigurationPointTypeIdAndHeights, System.Drawing.Point startPoint, System.Drawing.Size gridDivisions, int gridDistance)
		{
			if (demConfigurationPointTypeIdAndHeights.Length == 0)
				return;

			CreatePoint2Altitude(demConfigurationPointTypeIdAndHeights, startPoint, gridDivisions);
		}

		/// <summary>
		/// _point2GridPointInfo から指定された格子点の GridPointInfo を取得する。
		/// </summary>
		/// <param name="x">格子点の x 座標</param>
		/// <param name="y">格子点の y 座標</param>
		/// <returns>存在する場合は値を返し、ない場合は null を返す。</returns>
		internal GridPointInfo? GridPointInfo(int x, int y) =>
			GridPointInfo(new(x, y));

		/// <summary>
		/// _point2GridPointInfo から指定された格子点の GridPointInfo を取得する。
		/// </summary>
		/// <param name="key">格子点の座標</param>
		/// <returns>存在する場合は値を返し、ない場合は null を返す。</returns>
		internal GridPointInfo? GridPointInfo(System.Drawing.Point key) =>
			_point2GridPointInfo.ContainsKey(key)
				? _point2GridPointInfo[key]
				: null;

		/// <summary>
		/// bmp イメージの配列を生成する。
		/// </summary>
		/// <param name="gridDivisions">格子の分割数</param>
		/// <param name="cancellationToken">キャンセル用オブジェクト</param>
		/// <returns>bmp イメージの配列</returns>
		internal BitmapSource CreateBitmap(System.Drawing.Size gridDivisions, CancellationToken cancellationToken)
		{
			return GmlHelpers.s_BitmapSetingHeightmap.CreateBitmap((x, y) => GridPointInfo(x, y)?.HeightmapColor, gridDivisions, cancellationToken);
		}

		/// <summary>
		/// Create の補助関数。
		/// _point2GridPointInfo の生成を行う。
		/// </summary>
		/// <param name="demConfigurationPointTypeIdAndHeights">Gml ファイルの tupleList に設定されている格子点に関する情報</param>
		/// <param name="startPoint">データの開始位置</param>
		/// <param name="gridDivisions">格子の分割数</param>
		private void CreatePoint2Altitude((DemConfigurationPointType, double)[] demConfigurationPointTypeIdAndHeights, System.Drawing.Point startPoint, System.Drawing.Size gridDivisions)
		{
			var sx = startPoint.X;
			var sy = startPoint.Y;
			var mx = gridDivisions.Width;
			var my = gridDivisions.Height;
			int t = 0;
			for (int y = sy; y < my; ++y)
			{
				for (int x = sx; x < mx; ++x)
				{
					var (id, height) = demConfigurationPointTypeIdAndHeights[t++];
					var gridPointInfo = Gml.GridPointInfo.Create(id, height);
					if (gridPointInfo != null)
					{
						_point2GridPointInfo.Add(new(x, y), gridPointInfo);
					}
					if (t == demConfigurationPointTypeIdAndHeights.Length)
					{
						return;
					}
				}
				sx = 0;
			}
		}
	}
}
