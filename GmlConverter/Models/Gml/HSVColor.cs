//namespace GmlConverter.Models.Gml
//{
//	/// <summary>
//	/// HSV カラーを表現するためのクラス
//	/// </summary>
//	internal struct HSVColor
//	{
//		/// <summary>
//		/// Hue(色相)
//		/// 0 <= h && h < 360/360.0
//		/// </summary>
//		internal double H;

//		/// <summary>
//		/// Saturation(彩度)
//		/// 0 <= s && s < 1
//		/// </summary>
//		internal double S;

//		/// <summary>
//		/// Value(明度)
//		/// 0 <= v && v < 1
//		/// </summary>
//		internal double V;

//		/// <summary>
//		/// コンストラクタ
//		/// </summary>
//		/// <param name="h">色相</param>
//		/// <param name="s">彩度</param>
//		/// <param name="v">明度</param>
//		internal HSVColor(double h, double s, double v)
//		{
//			H = h;
//			S = s;
//			V = v;
//		}

//		/// <summary>
//		/// RGB カラーへの変換関数。
//		/// (S == 0) && (V == 0) の場合は透明色を返す。
//		/// </summary>
//		/// <param name="hsvColor">変換したい HSV 色</param>
//		/// <returns>RGB 色</returns>
//		internal static System.Drawing.Color CreateColor(HSVColor hsvColor)
//		{
//			if (hsvColor.S > 0.0)
//			{
//				var h_ = hsvColor.H * 360/ 60.0;
//				var hZone = (int)h_;
//				var f = h_ - hZone;
//				var v = (int)(hsvColor.V * 255);
//				var p = (int)(v * (1.0 - hsvColor.S));
//				var q = (int)(v * (1.0 - hsvColor.S * f));
//				var t = (int)(v * (1.0 - hsvColor.S * (1.0 - f)));

//				var (r, g, b) = hZone switch
//				{
//					0 => (v, t, p),
//					1 => (q, v, p),
//					2 => (p, v, t),
//					3 => (p, q, v),
//					4 => (t, p, v),
//					_ => (v, p, q),
//				};
//				return System.Drawing.Color.FromArgb(r, g, b);
//			}
//			else
//			{
//				var v = (int)(hsvColor.V * 255);
//				if (v > 0)
//					return System.Drawing.Color.FromArgb(v, v, v);
//				else
//					return System.Drawing.Color.FromArgb(0, 0, 0, 0);
//			}
//		}
//	}
//}