using GmlConverter.Utilities;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// 同じ二次メッシュに所属する Gml ファイルを集めるためのクラス。
	/// </summary>
	internal class SecondMeshContainer
	{
		//同じ mesh2 の Gml ファイル情報のリスト
		internal List<GmlFileInformation> GmlFileInformationList { get; set; }

		/// <summary>
		/// 現座登録されている gml ファイル群が存在する領域
		/// 最大10x10
		/// </summary>
		internal System.Drawing.Rectangle Rect { get; set; }

		/// <summary>
		/// 現在登録されている gml ファイル群のグリッド感距離の最大値と最小値
		/// </summary>
		internal MinMax<int> GridDistance { get; set; }

		/// <summary>
		/// 現在登録されている gml ファイル群の最小グリッド分割数
		/// 1[m] or 5[m] が設定されていれば 10x10
		/// 10[m] しか設定されていなければ 1x1
		/// </summary>
		internal System.Drawing.Size GridDivisions { get; set; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="gmlFileInformation">Gml ファイル情報</param>
		internal SecondMeshContainer(GmlFileInformation gmlFileInformation)
		{
			GmlFileInformationList = new() { gmlFileInformation };
			Rect = gmlFileInformation.GmlHeader.MeshNumber.Mesh3.GetRect();
			GridDistance = new(gmlFileInformation.GmlHeader.GridDistance);
			GridDivisions = gmlFileInformation.GmlHeader.GridDivisions;
		}

		/// <summary>
		/// 新規の Gml の登録
		/// </summary>
		/// <param name="gmlFileInformation">Gml ファイル情報</param>
		internal void Add(GmlFileInformation gmlFileInformation)
		{
			var rect = gmlFileInformation.GmlHeader.MeshNumber.Mesh3.GetRect();
			rect = System.Drawing.Rectangle.Union(rect, Rect);
			GmlFileInformationList.Add(gmlFileInformation);
			Rect = rect;
			var v = GridDistance.Min;
			GridDistance.Update(gmlFileInformation.GmlHeader.GridDistance);
			if (GridDistance.Min < v)
			{
				GridDivisions = gmlFileInformation.GmlHeader.GridDivisions;
			}
		}

		/// <summary>
		/// 描画領域矩形
		/// </summary>
		/// <returns></returns>
		internal System.Drawing.Size GetCanvasSize() =>
			GridDistance.Min == 10
				? GridDivisions
				: new(GridDivisions.Width * Rect.Size.Width, GridDivisions.Height * Rect.Size.Height);
	}
}
