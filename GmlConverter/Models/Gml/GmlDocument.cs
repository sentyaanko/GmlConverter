using System.Text.RegularExpressions;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// Gml ファイルの内容を表すクラスです。
	/// </summary>
	internal class GmlDocument
	{
		/// <summary>
		/// Gml のヘッダ情報
		/// </summary>
		internal GmlHeader GmlHeader;

		/// <summary>
		/// Gml のボディ情報
		/// </summary>
		internal GmlBody GmlBody;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="gmlHeader">ヘッダ情報</param>
		/// <param name="gmlBody">ボディ情報</param>
		private GmlDocument(GmlHeader gmlHeader, GmlBody gmlBody)
		{
			GmlHeader = gmlHeader;
			GmlBody = gmlBody;
		}

		/// <summary>
		/// GmlDocument の構築関数。
		/// </summary>
		/// <param name="xmlPath">xml ファイルのパス</param>
		/// <param name="cancellationToken">キャンセル用オブジェクト</param>
		/// <returns>正常に読めたときはオブジェクト、何らかのエラーがあった場合は null</returns>
		internal static GmlDocument? Create(string xmlPath, CancellationToken cancellationToken)
		{
			try
			{
				var node_Dataset_DEM_type =
					"/{0}:Dataset/{0}:DEM/{0}:type";
				var node_Dataset_DEM_mesh =
					"/{0}:Dataset/{0}:DEM/{0}:mesh";
				var node_Dataset_DEM_coverage_coverageFunction_GridFunction_startPoint =
					"/{0}:Dataset/{0}:DEM/{0}:coverage/gml:coverageFunction/gml:GridFunction/gml:startPoint";
				var node_Dataset_DEM_coverage_gridDomain_Grid_limits_GridEnvelope_high =
					"/{0}:Dataset/{0}:DEM/{0}:coverage/gml:gridDomain/gml:Grid/gml:limits/gml:GridEnvelope/gml:high";
				var node_Dataset_DEM_coverage_rangeSet_DataBlock_tupleList =
					"/{0}:Dataset/{0}:DEM/{0}:coverage/gml:rangeSet/gml:DataBlock/gml:tupleList";

				var xmlHolder = XmlHelpers.LoadXml(GmlHelpers.XsdPath, xmlPath);
				if (xmlHolder == null)
				{
					return null;
				}

				cancellationToken.ThrowIfCancellationRequested();

				//type の取得
				var demTypeName = xmlHolder.GetSingleNodeInnerText(node_Dataset_DEM_type);
				if (demTypeName == null)
				{
					return null;
				}

				//type の取得
				var demType = DemTypeExt.Search(demTypeName);
				if (demType == DemType.Error)
				{
					return null;
				}
				var gridDistance = DemTypeExt.GetGridDistance(demType);

				//startPoint の取得
				var stringStartPoint = xmlHolder.GetSingleNodeInnerText(node_Dataset_DEM_coverage_coverageFunction_GridFunction_startPoint);
				if (stringStartPoint == null)
				{
					return null;
				}
				var startPoint_ = SplitPoint(stringStartPoint);
				if (startPoint_ == null)
				{
					return null;
				}
				var startPoint = startPoint_.Value;

				//gridDivisions の取得
				var stringHigh = xmlHolder.GetSingleNodeInnerText(node_Dataset_DEM_coverage_gridDomain_Grid_limits_GridEnvelope_high);
				if (stringHigh == null)
				{
					return null;
				}
				var gridDivisions_ = SplitSize(stringHigh);
				if (gridDivisions_ == null)
				{
					return null;
				}
				var gridDivisions = gridDivisions_.Value;
				//設定されている値は最終グリッドのインデックスのため、要素数にするため 1 足す。
				gridDivisions.Width += 1;
				gridDivisions.Height += 1;

				cancellationToken.ThrowIfCancellationRequested();

				//tupleList の取得
				var xmlTupleList = xmlHolder.GetSingleNodeInnerText(node_Dataset_DEM_coverage_rangeSet_DataBlock_tupleList);
				if (xmlTupleList == null)
				{
					return null;
				}
				var tupleLists = SplitStrings(xmlTupleList);
				(DemConfigurationPointType, double)[] demConfigurationPointTypeIdAndHeights = Array.ConvertAll(tupleLists, SplitDemConfigurationPointTypeIdAndHeight);

				GmlBody gmlBody = new(demConfigurationPointTypeIdAndHeights, startPoint, gridDivisions, gridDistance);

				cancellationToken.ThrowIfCancellationRequested();

				//meshNumber の取得
				var stringMesh = xmlHolder.GetSingleNodeInnerText(node_Dataset_DEM_mesh);
				if (stringMesh == null)
				{
					return null;
				}
				var meshNumber = MeshNumber.Create(stringMesh, demType);
				if (meshNumber == null)
				{
					return null;
				}
				cancellationToken.ThrowIfCancellationRequested();

				return new(new(xmlPath, meshNumber, demTypeName, demType, gridDistance, gridDivisions), gmlBody);
			}
			catch (OperationCanceledException e)
			{
				Console.WriteLine(e);
				throw;
			}
			catch (Exception e)// when (e is not OperationCanceledException)
			{
				Console.WriteLine(e);
			}
			return null;
		}

		/// <summary>
		/// スペースで分割された整数 2 つからなる文字列をタプルで返却する。
		/// </summary>
		/// <param name="str">変換する文字列</param>
		/// <returns>正常に変換できた場合はタプル、何らかのエラーがあった場合は null</returns>
		private static (int, int)? SplitTwoInts(string str)
		{
			if (!Regex.IsMatch(str, "^[0-9]+ +[0-9]+$"))
			{
				return null;
			}
			var xy = str.Split(' ');
			return (int.Parse(xy[0]), int.Parse(xy[1]));
		}

		/// <summary>
		/// スペースで分割された整数 2 つからなる文字列を System.Drawing.Point で返却する。
		/// </summary>
		/// <param name="str">変換する文字列</param>
		/// <returns>正常に変換できた場合は System.Drawing.Point 、何らかのエラーがあった場合は null</returns>
		private static System.Drawing.Point? SplitPoint(string str)
		{
			var v = SplitTwoInts(str);
			if (v == null)
			{
				return null;
			}
			var (x, y) = v.Value;
			return new(x, y);
		}

		/// <summary>
		/// スペースで分割された整数 2 つからなる文字列を System.Drawing.Size で返却する。
		/// </summary>
		/// <param name="str">変換する文字列</param>
		/// <returns>正常に変換できた場合は System.Drawing.Size 、何らかのエラーがあった場合は null</returns>
		private static System.Drawing.Size? SplitSize(string str)
		{
			var v = SplitTwoInts(str);
			if (v == null)
			{
				return null;
			}
			var (width, height) = v.Value;
			return new(width, height);
		}

		/// <summary>
		/// 改行コードを含む文字列を改行コードごとに分割して返却する。
		/// </summary>
		/// <param name="str">変換する文字列</param>
		/// <returns>改行コードで分割された文字列の配列</returns>
		private static string[] SplitStrings(string str)
		{
			char[] splits = { '\r', '\n', };
			return str.Split(splits, StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		/// スペースで分割された "DEM構成点種別列挙型" と 標高を示す実数 からなる文字列をタプルで返却する。
		/// 行頭の空白文字列は削除される。
		/// </summary>
		/// <param name="str">変換する文字列</param>
		/// <returns>"DEM構成点種別列挙型" と 標高 のタプル</returns>
		private static (DemConfigurationPointType, double) SplitDemConfigurationPointTypeIdAndHeight(string str)
		{
			str.TrimStart();
			var values = str.Split(',');
			if (values.Length == 2)
			{
				double height;
				if (double.TryParse(values[1], out height))
				{
					DemConfigurationPointType id = DemConfigurationPointTypeExt.Search(values[0]);
					if (id != DemConfigurationPointType.Error)
					{
						return (id, height);
					}
				}
			}
			return (DemConfigurationPointType.Error, GmlHelpers.ErrorHeight);
		}
	}
}
