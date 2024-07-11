namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// "DEM構成点種別列挙型" 関連のヘルパークラスです。
	/// </summary>
	internal static class DemConfigurationPointTypeExt
	{
		/// <summary>
		/// "DEM構成点種別列挙型" の文字列から enum に変換する際に使用する辞書です。
		/// </summary>
		private static Dictionary<string, DemConfigurationPointType> s_string2DemConfigurationPointTypeID = new()
		{
			{"地表面",     DemConfigurationPointType.Ground},
			{"表層面",     DemConfigurationPointType.Surface},
			{"海水面",     DemConfigurationPointType.Seawater},
			{"内水面",     DemConfigurationPointType.InlandWater},
			{"データなし", DemConfigurationPointType.NoData},
			{"その他",     DemConfigurationPointType.Other },
		};

		/// <summary>
		/// "DEM構成点種別列挙型" の文字列から enum に変換するための関数です。
		/// </summary>
		/// <param name="name">"DEM構成点種別列挙型" の文字列</param>
		/// <returns>"DEM構成点種別列挙型" を示す enum</returns>
		internal static DemConfigurationPointType Search(string name) =>
			s_string2DemConfigurationPointTypeID.ContainsKey(name)
				? s_string2DemConfigurationPointTypeID[name]
				: DemConfigurationPointType.Error;
	}

	/// <summary>
	/// Gml に設定されている "DEM構成点種別列挙型" を示す enum です。
	/// </summary>
	internal enum DemConfigurationPointType
	{
		Ground,
		Surface,
		Seawater,
		InlandWater,
		NoData,
		Other,
		Error,
	};
}