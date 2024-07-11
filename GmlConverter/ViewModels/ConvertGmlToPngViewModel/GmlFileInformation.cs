using GmlConverter.Models.Gml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace GmlConverter.ViewModels
{
	/// <summary>
	/// GmlFileInformation Class
	/// Requires using System.ComponentModel;
	/// </summary>
	internal class GmlFileInformation : INotifyPropertyChanged/*, IComparable*/ /*, IEditableObject */
	{
		// The GmlFileInformation class implements INotifyPropertyChanged and IEditableObject
		// so that the datagrid can properly respond to changes to the
		// data collection and edits made in the DataGrid.

		/// <summary>
		/// Implement INotifyPropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		// Create the OnPropertyChanged method to raise the event
		// The calling member's name will be used as the parameter.
		protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

		/// <summary>
		/// 有効状態（コンバート対象かどうか）
		/// </summary>
		private bool _active;
		public bool Active
		{
			get => _active;
			set
			{
				if (value != _active)
				{
					_active = value;
					OnPropertyChanged();
				}
			}
		}

		public string DisplayName { get; private set; }
		public string MeshArea1 { get; private set; }
		public string MeshArea2 { get; private set; }
		public BitmapSource HeightmapBitmapSource { get; private set; }
		public string Mesh1 { get => GmlHeader.MeshNumber.Mesh1.ToString() ?? string.Empty; }
		public string Mesh2 { get => GmlHeader.MeshNumber.Mesh2.ToString() ?? string.Empty; }
		public string Mesh3 { get => GmlHeader.MeshNumber.Mesh3.ToString() ?? string.Empty; }
		public string DemTypeName { get => GmlHeader.DemTypeName; }

		/// <summary>
		/// Gml のヘッダ情報
		/// </summary>
		internal GmlHeader GmlHeader { get; private set; }


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="gmlHeader">Gml のヘッダ情報</param>
		/// <param name="heightmapBitmapSource">高度マップ用ビットマップ</param>
		private GmlFileInformation(GmlHeader gmlHeader, BitmapSource heightmapBitmapSource)
		{
			var displayName = System.IO.Path.GetFileName(gmlHeader.FileName) ?? string.Empty;

			_active = true;
			GmlHeader = gmlHeader;
			DisplayName = displayName;
			MeshArea1 = $"{gmlHeader.MeshNumber.Mesh1.ToString()}";
			MeshArea2 = $"{gmlHeader.MeshNumber.Mesh2.ToString()}";
			HeightmapBitmapSource = heightmapBitmapSource;
		}

		/// <summary>
		/// ファクトリ関数
		/// </summary>
		/// <param name="xmlPath">Gml ファイルのパス</param>
		/// <param name="cancellationToken">キャンセル用オブジェクト</param>
		/// <returns></returns>
		internal static GmlFileInformation? Create(string xmlPath, CancellationToken cancellationToken)
		{
			var gmlDocument = GmlDocument.Create(xmlPath, cancellationToken);
			if (gmlDocument == null)
			{
				return null;
			}
			cancellationToken.ThrowIfCancellationRequested();

			var heightmap = gmlDocument.GmlBody.CreateBitmap(gmlDocument.GmlHeader.GridDivisions, cancellationToken);
			return new(gmlDocument.GmlHeader, heightmap);
		}
	}
}