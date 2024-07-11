namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// n次メッシュ番号を扱うクラス
	/// </summary>
	internal class MeshNumberUnit
	{
		/// <summary>
		/// n次メッシュ番号保持用
		/// </summary>
		private int _number;

		/// <summary>
		/// n次メッシュ番号の桁数保持用
		/// </summary>
		private int _digit;

		/// <summary>
		/// n-1次メッシュの分割数
		/// Dem10 系の mesh3 の場合は 1 となる 
		/// </summary>
		private int _div;

		/// <summary>
		/// n次メッシュ番号の経度成分保持用。無効の場合は -1
		/// </summary>
		private int _x;

		/// <summary>
		/// n次メッシュ番号の緯度成分保持用。無効の場合は -1
		/// </summary>
		private int _y;

		/// <summary>
		/// n次メッシュ番号
		/// </summary>
		internal int Number { get => _number; }

		/// <summary>
		/// n次メッシュ番号の経度成分
		/// </summary>
		internal int X { get => _x; }

		/// <summary>
		/// n次メッシュ番号の緯度成分
		/// </summary>
		internal int Y { get => _y; }

		/// <summary>
		/// n次メッシュ番号が有効かどうか。有効(0 以上)なら true
		/// </summary>
		internal bool IsActive { get => _number >= 0; }

		/// <summary>
		/// 矩形を取得
		/// </summary>
		/// <returns>Number が -1 の場合は全領域、そうでなければ、 {Left, Bottom, 1, 1}</returns>
		internal System.Drawing.Rectangle GetRect() =>
			Number < 0
				? new(0, 0, _div, _div)
				: new(X, Y, 1, 1);

		/// <summary>
		/// 位置を取得
		/// </summary>
		/// <returns>Number が -1 の場合は原点、そうでなければ、 {Left, Bottom}</returns>
		internal System.Drawing.Point GetPoint() =>
			Number < 0
				? new(0, 0)
				: new(X, Y);

		/// <summary>
		/// 描画位置を取得
		/// </summary>
		/// <param name="rect">キャンバス矩形(0 <= x,y < 10, 1 <= w,h <= 10)</param>
		/// <returns>キャンバス矩形内の描画位置（原点は左上、単位はメッシュ）</returns>
		internal System.Drawing.Point GetRenderPoint(System.Drawing.Rectangle rect)
		{
			var point = GetPoint();
			var meshRect = GetRect();
			return new(point.X - rect.X, rect.Y + rect.Height - meshRect.Height - point.Y);
		}

		/// <summary>
		/// 表示用の文字列化
		/// </summary>
		/// <returns>表示用文字列</returns>
		public override string? ToString() =>
		IsActive
				? _number.ToString().PadLeft(_digit, '0')
				: string.Empty.PadLeft(_digit, '-');

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="number">n次メッシュ番号</param>
		/// <param name="digit">number の桁数。 2 または 4</param>
		/// <param name="div">n-1次メッシュの分割数</param>
		/// 
		internal MeshNumberUnit(int number, int digit, int div)
		{
			if (digit != 2 && digit != 4)
			{
				return;
			}
			_number = number;
			_digit = digit;
			_div = div;
			if (number < 0)
			{
				_x = -1;
				_y = -1;
			}
			else
			{
				var divXY = digit == 2 ? 10 : 100;
				_x = number % divXY;
				_y = number / divXY;
			}
		}
	}
}
