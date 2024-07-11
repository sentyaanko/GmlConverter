namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// "DEM種別列挙型" 関連のヘルパークラスです。
	/// </summary>
	internal static class DemTypeExt
	{
		/// <summary>
		/// "DEM種別列挙型" の文字列から enum に変換する際に使用する辞書です。
		/// </summary>
		private static Dictionary<string, DemType> s_string2DemTypeID = new()
		{
			{"1mメッシュ（標高）",      DemType.Dem1A},
			{"5mメッシュ（数値地形）",  DemType.Dem5B},
			{"5mメッシュ（標高）",      DemType.Dem5A},
			{"10mメッシュ（標高）",     DemType.Dem10B},
			{"10mメッシュ（火山標高）", DemType.Dem10A},
		};

		/// <summary>
		/// "DEM種別列挙型" の文字列から enum に変換するための関数です。
		/// </summary>
		/// <param name="name">"DEM種別列挙型" の文字列</param>
		/// <returns>"DEM種別列挙型" を示す enum</returns>
		internal static DemType Search(string name) =>
			s_string2DemTypeID.ContainsKey(name)
				? s_string2DemTypeID[name]
				: DemType.Error;

		/// <summary>
		/// DemType に関連するデータ用
		/// </summary>
		private class DemTypeIDDetail
		{
			/// <summary>
			/// 格子点間の距離
			/// </summary>
			internal int Distance { get; set; }

			/// <summary>
			/// mesh の文字数
			/// </summary>
			internal int StringLength { get; set; }

			/// <summary>
			/// 描画プライオリティ
			/// </summary>
			internal int RenderPriority { get; set; }

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="distance"></param>
			/// <param name="stringLength"></param>
			/// <param name="renderPriority"></param>
			internal DemTypeIDDetail(int distance, int stringLength, int renderPriority)
			{
				Distance = distance;
				StringLength = stringLength;
				RenderPriority = renderPriority;
			}
		};

		/// <summary>
		/// DemType に関連するデータ用テーブル
		/// </summary>
		private static DemTypeIDDetail[] s_demTypeDemTypeIDDetails =
		{
			new( 1,  8,  0),	// Dem1A:	1mメッシュ（標高）
			new( 5,  8,  2),	// Dem5B:	5mメッシュ（数値地形）
			new( 5,  8,  1),	// Dem5A:	5mメッシュ（標高）
			new(10,  6,  4),	// Dem10B:	10mメッシュ（標高）
			new(10,  6,  3),	// Dem10A:	10mメッシュ（火山標高）
			new(-1, -1, -1),
		};

		/// <summary>
		/// "DEM種別列挙型" からセルサイズ（格子点間の距離）に変換する関数です。
		/// </summary>
		/// <param name="demType">"DEM種別列挙型" を示す enum</param>
		/// <returns>セルサイズ</returns>
		internal static int GetGridDistance(DemType demType) =>
			s_demTypeDemTypeIDDetails[(int)demType].Distance;

		/// <summary>
		/// "DEM種別列挙型" から mesh の文字数に変換する関数です。
		/// </summary>
		/// <param name="demType">"DEM種別列挙型" を示す enum</param>
		/// <returns>mesh の文字数</returns>
		internal static int GetMeshStringLength(DemType demType) =>
			s_demTypeDemTypeIDDetails[(int)demType].StringLength;

		/// <summary>
		/// "DEM種別列挙型" から描画プライオリティに変換する関数です。
		/// </summary>
		/// <param name="demType">"DEM種別列挙型" を示す enum</param>
		/// <returns>描画優先順位（大きいほど先に描画）</returns>
		internal static int GetRenerPriority(DemType demType) =>
			s_demTypeDemTypeIDDetails[(int)demType].RenderPriority;
	};

	/// <summary>
	/// Gml に設定されている "DEM種別列挙型" を示す enum です。
	/// </summary>
	internal enum DemType
	{
		Dem1A,          // Dem1A:	1mメッシュ（標高）
		Dem5B,          // Dem5B:	5mメッシュ（数値地形）
		Dem5A,          // Dem5A:	5mメッシュ（標高）
		Dem10B,         // Dem10B:	10mメッシュ（標高）
		Dem10A,         // Dem10A:	10mメッシュ（火山標高）
		Error,
	};
}