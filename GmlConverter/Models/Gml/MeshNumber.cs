using System.Text.RegularExpressions;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// メッシュ番号を扱うクラス
	/// </summary>
	internal class MeshNumber
	{
		/// <summary>
		/// 一次メッシュの番号保持用
		/// </summary>
		private MeshNumberUnit _mesh1;

		/// <summary>
		/// 二次メッシュの番号保持用
		/// </summary>
		private MeshNumberUnit _mesh2;

		/// <summary>
		/// 三次メッシュの番号保持用
		/// </summary>
		private MeshNumberUnit _mesh3;

		/// <summary>
		/// 一次メッシュの番号
		/// </summary>
		internal MeshNumberUnit Mesh1 { get => _mesh1; }

		/// <summary>
		/// 二次メッシュの番号
		/// </summary>
		internal MeshNumberUnit Mesh2 { get => _mesh2; }

		/// <summary>
		/// 三次メッシュの番号
		/// </summary>
		internal MeshNumberUnit Mesh3 { get => _mesh3; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="mesh1">一次メッシュの番号</param>
		/// <param name="mesh2">二次メッシュの番号</param>
		/// <param name="mesh3">三次メッシュの番号</param>
		private MeshNumber(MeshNumberUnit mesh1, MeshNumberUnit mesh2, MeshNumberUnit mesh3)
		{
			_mesh1 = mesh1;
			_mesh2 = mesh2;
			_mesh3 = mesh3;
		}

		/// <summary>
		/// MeshNumber のファクトリ関数
		/// </summary>
		/// <param name="number">メッシュ番号</param>
		/// <param name="demType">メッシュ種別</param>
		/// <returns>値が有効な場合はメッシュ番号クラス、エラーの場合は null</returns>
		internal static MeshNumber? Create(string number, DemType demType)
		{
			var meshStringLength = DemTypeExt.GetMeshStringLength(demType);
			if (number.Length != meshStringLength)
			{
				return null;
			}
			if (!Regex.IsMatch(number, "^[0-9]+$"))
			{
				return null;
			}
			int tmp;
			if (!int.TryParse(number, out tmp))
			{
				return null;
			}
			var (mesh1, mesh2, mesh3) = meshStringLength switch
			{
				6 => (tmp / 100, tmp % 100, -1),
				8 => (tmp / 10000, tmp / 100 % 100, tmp % 100),
				_ => (-1, -1, -1),
			};
			return new(new(mesh1, 4, -1), new(mesh2, 2, 8), new(mesh3, 2, 10));
		}
	}
}
