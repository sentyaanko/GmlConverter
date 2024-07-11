using GmlConverter.Models.Gml;
using ImageMagick;

namespace GmlConverter.ViewModels
{
	internal class DistanceMap : IDisposable
	{
		private int _width = 0;
		private int _height = 0;
		private float[]? _distanceMap = null;
		private MagickImage? _image = null;
		public MagickImage? Image
		{
			get => _image;
		}
		private double _slopeDepthScale= 100;
		private double _slopeDistanceScale= 100;
		private double _slopeInitialDepth= 0;


		internal DistanceMap()
		{
		}

		~DistanceMap() => Dispose();

		public void Dispose()
		{
			_image?.Dispose();
			_image = null;
		}

		internal void SetData(int width, int height, float[]? distanceMap, MagickImage? image, double slopeDepthScale, double slopeDistanceScale, double slopeInitialDepth)
		{
			Dispose();
			_width = width;
			_height = height;
			_distanceMap = distanceMap;
			_image = image;
			_slopeDepthScale = slopeDepthScale;
			_slopeDistanceScale = slopeDistanceScale;
			_slopeInitialDepth = slopeInitialDepth;
		}

		internal bool IsChangedSlopeSettings(double slopeDepthScale, double slopeDistanceScale, double slopeInitialDepth)
		{
			return 
				_slopeDepthScale != slopeDepthScale || 
				_slopeDistanceScale != slopeDistanceScale ||
				_slopeInitialDepth != slopeInitialDepth;
		}

		internal void Update(MagickImage inputMagickImage, AngleMap angleMap, double slopeDepthScale, double slopeDistanceScale, double slopeInitialDepth)
		{
			SetData(0, 0, null, null, 0, 0, 0);

			var width = inputMagickImage.Width;
			var height = inputMagickImage.Height;
			//戻り値で使うので using しない
			var clone = inputMagickImage.Clone() as MagickImage;
			if (clone == null)
			{
				throw new Exception();
			}
			using var pixels = clone.GetPixelsUnsafe();
			var pixcelDataGray16 = pixels.ToArray();
			if (pixcelDataGray16 == null)
			{
				throw new Exception();
			}

			{
				using var dist = DistanceTransform(pixcelDataGray16, width, height);

				double minVal, maxVal;
				dist.MinMaxIdx(out minVal, out maxVal);

				//出力用。
				float[] distanceMap = new float[width * height];
				dist.GetArray(out distanceMap);

				UpdatePixels(pixcelDataGray16, width, height, angleMap, dist, distanceMap, minVal, maxVal, slopeDepthScale, slopeDistanceScale, slopeInitialDepth);
				pixels.SetPixels(pixcelDataGray16);

				SetData(width, height, distanceMap, clone, slopeDepthScale, slopeDistanceScale, slopeInitialDepth);
			}
		}
		private OpenCvSharp.Mat<float> DistanceTransform(ushort[] pixcelDataGray16, int width, int height)
		{
			//高さマップから mat を作る
			using var src16bit = OpenCvSharp.Mat.FromPixelData(height, width, OpenCvSharp.MatType.CV_16UC1, pixcelDataGray16);

			//byte の mat を作る(DistanceTransform に渡す用)
			using var invBinary = new OpenCvSharp.Mat(width, height, OpenCvSharp.MatType.CV_8UC1);

			//invBinary[n] = src16bit[n]? 0: 1; のような変換をする
			//ConvertTo() の挙動
			// invBinary(x,y) = saturate( alpha * src16bit(x,y) + beta );
			src16bit.ConvertTo(invBinary, OpenCvSharp.MatType.CV_8UC1, -1, 1);

			//非 0 のピクセルから 0 のピクセルまでの距離を測る
			//	DistanceTypes.L1: distance = |x1-x2| + |y1-y2|  [CV_DIST_L1]
			//	DistanceTypes.L2: the simple euclidean distance  [CV_DIST_L2]
			return invBinary.DistanceTransform(OpenCvSharp.DistanceTypes.L1, OpenCvSharp.DistanceTransformMasks.Mask5);
		}

		private void UpdatePixels(ushort[] pixcelDataGray16, int width, int height, AngleMap angleMap, OpenCvSharp.Mat<float> dist, float[] distanceMap, double minVal, double maxVal, double slopeDepthScale, double slopeDistanceScale, double slopeInitialDepth)
		{
			var dic = new Dictionary<int, List<int>>();
			{
				int i = 0;
				foreach (var distance in distanceMap)
				{
					int key = (int)distance;
					if (key != 0)
					{
						if (dic.ContainsKey(key))
							dic[key].Add(i);
						else
							dic[key] = [i];
					}
					++i;
				}
			}
			var max = (int)maxVal;
			for(int d = (int)minVal; d <= max; ++d)
			{
				if (d == 0 || !dic.ContainsKey(d))
					continue;
				var indexes = dic[d];
				if (indexes == null)
					continue;

				var depth = slopeDepthScale * Math.Log(1 + (d-1) * angleMap.PixelDistance / slopeDistanceScale) + slopeInitialDepth;

				var depthMax = 200.0;
				var alpha = depth / depthMax;

				if (d == 1)
				{
					//陸地に隣接している海は水面の標高とする。
					using (var e = indexes.GetEnumerator())
					{
						var h = GmlHelpers.HightToGrayscale(-depth);
						while (e.MoveNext())
						{
							var idx = e.Current;
							var x = idx % width;
							var y = idx / width;
							pixcelDataGray16[idx] = h;
						}
					}
					using (var e = indexes.GetEnumerator())
					{
						while (e.MoveNext())
						{
							var idx = e.Current;
							var x = idx % width;
							var y = idx / width;
							angleMap.UpdateWidthPosition(pixcelDataGray16, x, y);
						}
					}
				}
				else if (alpha < 1)
				{
					//depth が depthMax (=200 m) になるまでは陸地の傾きと depth の値を混ぜて使う。
					using (var e = indexes.GetEnumerator())
					{
						var oneMinusAlpha = 1 - alpha;
						var depthAlpha = -depth * alpha;
						while (e.MoveNext())
						{
							var idx = e.Current;
							var x = idx % width;
							var y = idx / width;
							var h = angleMap.CalculateHeightFromAngles(pixcelDataGray16, x, y);
							pixcelDataGray16[idx] = GmlHelpers.HightToGrayscale(h * oneMinusAlpha + depthAlpha);
						}
					}
					using (var e = indexes.GetEnumerator())
					{
						while (e.MoveNext())
						{
							var idx = e.Current;
							var x = idx % width;
							var y = idx / width;
							angleMap.UpdateWidthPosition(pixcelDataGray16, x, y);
						}
					}
				}
				else
				{
					//depth >= depthMax(=200) 以降は depthMax 固定
					using (var e = indexes.GetEnumerator())
					{
						while (e.MoveNext())
						{
							var idx = e.Current;
							var x = idx % width;
							var y = idx / width;
							pixcelDataGray16[idx] = GmlHelpers.HightToGrayscale(-depthMax);
						}
					}
				}
			}
		}
	}
}
