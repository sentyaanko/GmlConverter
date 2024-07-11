namespace GmlConverter.Utilities
{
	/// <summary>
	/// 最小値と最大値保存クラス
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class MinMax<T> where T : IComparable<T>
	{
		private T _min;
		private T _max;
		internal T Min { get => _min; }
		internal T Max { get => _max; }

		internal MinMax(T value)
		{
			_min = value;
			_max = value;
		}

		internal void Update(T value)
		{
			if (value.CompareTo(_min) < 0)
				_min = value;
			if (value.CompareTo(_max) > 0)
				_max = value;
		}
	}
}
