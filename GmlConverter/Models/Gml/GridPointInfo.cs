namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// 格子点情報
	/// </summary>
	internal class GridPointInfo
	{
		/// <summary>
		/// "DEM構成点種別列挙型"
		/// </summary>
		private DemConfigurationPointType _demConfigurationPointType;

		/// <summary>
		/// 高度
		/// </summary>
		private double _height;

		/// <summary>
		/// 高度マップの色
		/// </summary>
		internal System.Drawing.Color HeightmapColor;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="demConfigurationPointType">"DEM構成点種別列挙型"</param>
		/// <param name="height">高度</param>
		internal GridPointInfo(DemConfigurationPointType demConfigurationPointType, double height)
		{
			_demConfigurationPointType = demConfigurationPointType;
			_height = height;
			HeightmapColor = CreateHeightmapColor(demConfigurationPointType, height);
		}

		/// <summary>
		/// 地上かどうか。
		/// プレビューで陰影起伏情報を作成する対象化を判断するために使用。
		/// </summary>
		/// <returns>地上なら true</returns>
		internal bool IsGround() =>
			_demConfigurationPointType switch
			{
				DemConfigurationPointType.Ground or
				DemConfigurationPointType.Surface or
				DemConfigurationPointType.Other => true,
				_ => false,
			};

		/// <summary>
		/// 高度の取得。
		/// </summary>
		/// <returns>値が設定されている場合はその値、海の場合は海の値、エラー時はエラーの値</returns>
		internal double GetHight() =>
			_demConfigurationPointType switch
			{
				DemConfigurationPointType.Ground or
				DemConfigurationPointType.Surface or
				DemConfigurationPointType.Other => _height,
				DemConfigurationPointType.Seawater or
				DemConfigurationPointType.InlandWater => GmlHelpers.SeaHeight,
				_ => GmlHelpers.ErrorHeight,
			};

		/// <summary>
		/// "DEM構成点種別列挙型" と高度から高度マップ用画像の色を取得する。
		/// </summary>
		/// <param name="demConfigurationPointType">"DEM構成点種別列挙型"</param>
		/// <param name="height">高度</param>
		/// <returns></returns>
		internal static System.Drawing.Color CreateHeightmapColor(DemConfigurationPointType demConfigurationPointType, double height) =>
			demConfigurationPointType switch
			{
				DemConfigurationPointType.Ground or
				DemConfigurationPointType.Surface or
				DemConfigurationPointType.Other => GmlHelpers.HightToHeightmapColor(height),
				DemConfigurationPointType.Seawater or
				DemConfigurationPointType.InlandWater => GmlHelpers.SeaColor,
				_ => GmlHelpers.ErrorColor,
			};

		/// <summary>
		/// GridPointInfo のファクトリ関数
		/// </summary>
		/// <param name="id">"DEM構成点種別列挙型"</param>
		/// <param name="height">高度</param>
		/// <returns></returns>
		internal static GridPointInfo? Create(DemConfigurationPointType id, double height) =>
			id switch
			{
				DemConfigurationPointType.Ground or
				DemConfigurationPointType.Surface => new(id, height),
				DemConfigurationPointType.Other => height != GmlHelpers.ErrorHeight ? new(id, height) : null,
				DemConfigurationPointType.Seawater or
				DemConfigurationPointType.InlandWater => new(id, GmlHelpers.SeaHeight),
				_ => null,
			};
	}
}
