namespace GmlConverter.ViewModels
{
	/// <summary>
	/// mesh 1,2,3 の一軸の値を保存するためのクラス
	/// </summary>
	internal class MeshLocationUnit : IComparable<MeshLocationUnit>
	{
		internal int Mesh1 { get; set; } = 0;
		internal int Mesh2 { get; set; } = 0;
		internal int Mesh3 { get; set; } = 0;

		internal MeshLocationUnit(int mesh1, int mesh2, int mesh3)
		{
			Mesh1 = mesh1;
			Mesh2 = mesh2;
			Mesh3 = mesh3;
		}

		public int CompareTo(MeshLocationUnit? other)
		{
			if (other == null)
				return 1;
			if (this == other)
				return 0;
			if (Mesh1 - other.Mesh1 is var diff1 && diff1 != 0)
				return diff1;
			if (Mesh2 - other.Mesh2 is var diff2 && diff2 != 0)
				return diff2;
			return Mesh3 - other.Mesh3;
		}
		public override string ToString()
			=> $"{Mesh1}-{Mesh2}-{Mesh3}";

		//Mesh1 が 10進、 Mesh2 が 8進 なのを踏まえて、 mesh2 の枚数を数える。
		internal int SubMesh2(MeshLocationUnit other)
			//=> ((Mesh1 - other.Mesh1) * 8) + (Mesh2 - other.Mesh2);
			=> (Mesh1 - other.Mesh1) * 8 + (Mesh2 - other.Mesh2);
	}
}
