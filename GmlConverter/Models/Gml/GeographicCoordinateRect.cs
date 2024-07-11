using System.Windows;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// 地理座標の矩形を表すクラス
	/// </summary>
	internal class GeographicCoordinateRect
	{
		/// <summary>
		/// 地理座標の矩形
		/// </summary>
		private Rect _rect;

		/// <summary>
		/// _mesh1 の 左下と右上の緯度経度を設定
		/// </summary>
		/// <param name="point">原点</param>
		/// <param name="vector">端点へのベクトル</param>
		internal GeographicCoordinateRect(Point point, Vector vector)
		{
			_rect = new(point, vector);
		}

		/// <summary>
		/// メッシュの原点の地理座標を取得
		/// 座標系が windows の「Left:右 Bottom:下」ではなく 「Left:右 Bottom:上」なので、 TopLeft を返す。
		/// </summary>
		internal Point OriginPoint { get => _rect.TopLeft; }

		/// <summary>
		/// メッシュの端点の地理座標を取得
		/// 座標系が windows の「Left:右 Bottom:下」ではなく 「Left:右 Bottom:上」なので、 BottomRight を返す。
		/// </summary>
		internal Point EndPoint { get => _rect.BottomRight; }

		/// <summary>
		/// 表示用の文字列化
		/// </summary>
		/// <returns>表示用文字列</returns>
		public override string? ToString() =>
			$"({OriginPoint.X:000.000000000},{OriginPoint.Y:00.000000000})-({EndPoint.X:000.000000000},{EndPoint.Y:00.000000000})";
	}
}
