using GmlConverter.Models.Gml;
using ImageMagick;
using System.Linq;
using static OpenCvSharp.LineIterator;

/*

削除済みの過去のコードに関するコメント。

# LinearGray について

* [https://imagemagick.org/script/command-line-options.php#colorspace]
	```LinearGray = 0.298839 * R + 0.586811 * G + 0.114350 * B```
* LinearGray にするということは上記の変換がかけられるということ。つまり当然色が変わる。
* また、 8bit に変換される


# 16bit PNG を開く際の指定

* 何も指定せずに開き、そのまま使うのが最善。
	* なので、 settings を渡して開くこともできるが、意味がない。
		```csharp
			var fmt = new MagickReadSettings() 
			{
				//ColorSpace = ColorSpace.LinearGray,
				ColorType = ColorType.Grayscale, 
				Format = MagickFormat.Png, Depth = 16 
			};
			MagickImage inputImage = new(filePath, fmt);
		```
	* こういう書き方はできない。(no pixels defined in cache `' @ error/cache.c/OpenPixelCache/3690)
		```csharp
			MagickImage inputImage = new();
			inputImage.ColorSpace = ColorSpace.LinearGray;	//そもそも LinearGray は用途が違う。
			inputImage.ColorType = ColorType.Grayscale;
			inputImage.Format = MagickFormat.Gray;			//Png が正しい。（Gray を指定しても意味ないはず）
			inputImage.Depth = 16;
			inputImage.Read(filePath, MagickFormat.Png);
		```
* そのまま開けば下記の出力が得られる。
	```csharp
		Debug.WriteLine($"input images ColorSpace is {inputImage.ColorSpace}.");		// Gray
		Debug.WriteLine($"input images ColorType is {inputImage.ColorType}.");			// Grayscale
		Debug.WriteLine($"input images Gamma is {inputImage.Gamma}.");					// 0.45454545454545453
		Debug.WriteLine($"input images Depthis {inputImage.Depth}.");					// 16
		Debug.WriteLine($"input images ChannelCount is {inputImage.ChannelCount}.");    // 1
	```
* Gamma は設定されているけど、 Grayscale の PNG はそういうものらしい。


# 16 bit PNG を G/B チャンネルに分けた BitmapSource を作る方法について

* 案 1
	* 概要
		```
			16bit 0b fedc ba98 7654 3210
			を
				r 0b eca8 6420
				g 0b fdb9 7531
			に分解して、シェーダー内で統合する方法。
			これにより、単純に r/g チャンネルに値を分けることで、 g チャンネルの値が ff -> 0 に急激に値が変わることを避け、
			サンプラーがおかしなブレンドされた値を取得してこないようにする。
		```
	* 結果
		```
			ps_3_0 ではビット演算は使用できないため、シェーダー内で値を復元できない。
			たかだか256種類なので、シェーダー内でテーブル持ってもいいのだが…。
		```
* 案 2
	* 概要
		```
			16bit grayscale をそのままシェーダーに渡す方法
			Gray16 は ToBitmapSource() では作れない。自前で作る必要がある。
		```
	* 結果
		```
			試したところ、どうにも変なエッジが発生する。
			よくわからないのでプログラムで出力した内容を表示してみたところ、
			下位バイトの情報が欠落する。
			（何故か上位バイトと同じ値になってしまう。）
			（16bit の上位バイトが上位下位両方に設定された状態で計算された値が取得される）
			 例：0x3c31 の値が、シェーダー内で取得すると 0x3c3c 相当の値(0x3c3c / 65535.0)に変わっている
		```
* 案 3
	* 概要
		```
			Evaluate を使い、16bit Grayscale を g:上位 b:下位 の 8bit grayscale に変換し、 MagickImage.Combine() でチャンネル統合する。
		```
	* 結果
		```
			工程のどこか(Conbine() もしくは ToBitmapSource() )でガンマ値の参照を行っているのか、色がぶっ壊れる。
			例： 0x6049 => 0x1007 など
		```

# 16 bit PNG を保存する方法について

* MagickImage の Fx を利用した変換。
	* ピクセル毎に処理するため、再計算が多く遅い。使用しない。
* 普通の変換。
	* Intrinsics 大差ないが、向こうを使うのでこちらは使わない。


 */


namespace GmlConverter.ViewModels
{
	/// <summary>
	/// 角度マップ
	/// 高さマップを元に、その地点と隣接地点をつなぐ斜面と水平面が成す角度を計算し保持する。
	/// 上下左右斜めの8方向を保持する。
	/// </summary>
	internal class AngleMap
	{
		internal int Width { get; set; } = 0;
		internal int Height { get; set; } = 0;
		internal double PixelDistance { get; set; } = 0;
		internal float[] Angles { get; set; } = [];

		enum ElementType
		{
			TopLeft, 
			Top,
			TopRight,
			Left,
			BottomRight,
			Bottom,
			BottomLeft,
			Right,
			Max,
		}

		internal AngleMap()
		{
		}

		internal void Update(MagickImage image, double pixelDistance)
		{
			Width = image.Width;
			Height = image.Height;
			PixelDistance = pixelDistance;
			if (Width == 0 || Height == 0 || PixelDistance == 0)
			{
				return;
			}
			using IUnsafePixelCollection<ushort> pixels = image.GetPixelsUnsafe();
			var pixcelDataGray16 = pixels.ToArray();
			if (pixcelDataGray16 == null)
			{
				throw new Exception();
			}

			Angles = new float[Width * Height * (int)ElementType.Max];
			var db = 0.1 / PixelDistance;
			var dh = db / Math.Sqrt(2);

			//ピクセル毎に 左上、上、右上、左 だけ計算する。
			int index = 0;
			for (int iY = 0; iY < Height; ++iY)
			{
				for (int iX = 0; iX < Width; ++iX)
				{
					Update0123(pixcelDataGray16, iX, iY, index, db, dh);
					index += (int)ElementType.Max;
				}
			}
			///残りは隣から符号反転した値を取得している。
			index = 4;
			for (int iY = 0; iY < Height; ++iY)
			{
				for (int iX = 0; iX < Width; ++iX)
				{
					Update8765(iX, iY, index);
					index += (int)ElementType.Max;
				}
			}
		}
		internal double CalculateHeightFromAngles(ushort[] pixcelDataGray16, int iX, int iY)
		{
			var isXMin = iX == 0;
			var isXMax = iX == (Width - 1);
			var isYMin = iY == 0;
			var isYMax = iY == (Height - 1);
			var line = Width * (int)ElementType.Max;
			var iIndex = iX + iY * Width;
			var index = iIndex * (int)ElementType.Max;

			var heightmapOrigin = GmlHelpers.HeightmapOrigin;
			//上下左右の標高
			double?[] udlrHeight =
				[
					isYMin? null: GmlHelpers.GrayscaleToHight(pixcelDataGray16[iIndex - Width]),
					isYMax? null: GmlHelpers.GrayscaleToHight(pixcelDataGray16[iIndex + Width]),
					isXMin? null: GmlHelpers.GrayscaleToHight(pixcelDataGray16[iIndex - 1]),
					isXMax? null: GmlHelpers.GrayscaleToHight(pixcelDataGray16[iIndex + 1]),
				];
			//上下左右の標高と傾きから予想した標高
			Func<double?, float[], int, double, double?> func = (height, angle, aindex, dist) => (height == null) ? null : (height - Math.Tan(angle[aindex]) * dist);
			double?[] udlr =
				[
					func(udlrHeight[0], Angles, index - line + (int)ElementType.Top, PixelDistance),
					func(udlrHeight[1], Angles, index + line + (int)ElementType.Bottom, PixelDistance),
					func(udlrHeight[2], Angles, index - (int)ElementType.Max + (int)ElementType.Left, PixelDistance),
					func(udlrHeight[3], Angles, index + (int)ElementType.Max + (int)ElementType.Right, PixelDistance)
				];
			// h > 0 は、データなし領域に向かって標高が上がるケース。
			var ave = udlr.Where(h => (h != null && h <= 0 && h > GmlHelpers.HeightmapOrigin)).DefaultIfEmpty().Average() ?? GmlHelpers.HeightmapOrigin;
			if (ave != GmlHelpers.HeightmapOrigin)
				return ave;
			//□ が 0 の領域
			//■ が 非0 の領域
			//◯ が 処理中の座標 とすると
			//
			//□■□□□
			//□■◯□□
			//□□□□□
			//
			//上記のようなケースだと、 Angles から傾きが取れず、 ave が GmlHelpers.MinHeight になる。
			//その場合は上下左右の標高の平均を返す
			//平均を取れないケースはないはずだが、取れない場合は GmlHelpers.MinHeight を返す
			return udlrHeight.Where(h => (h != null && h != GmlHelpers.HeightmapOrigin)).DefaultIfEmpty().Average() ?? GmlHelpers.HeightmapOrigin;
		}
		internal void UpdateWidthPosition(ushort[] pixcelDataGray16, int iX, int iY)
		{
			var db = 0.1 / PixelDistance;
			
			//右と下の高さが設定してある場合は、自身に対する傾きを更新する。
			if (iX != Width - 1)
			{
				var iIndex = iX + iY * Width + 1;
				var v4 = pixcelDataGray16[iIndex];
				if(v4 != 0)
				{
					var diff = pixcelDataGray16[iIndex - 1] - v4;
					var index = iIndex * (int)ElementType.Max + (int)ElementType.Left;
					Angles[index] = (diff == 0) ? 0 : (float)Math.Atan(db * diff);
				}
			}
			if (iY != Height - 1)
			{
				var iIndex = iX + iY * Width + Width;
				var v4 = pixcelDataGray16[iIndex];
				if (v4 != 0)
				{
					var diff = pixcelDataGray16[iIndex - Width] - v4;
					var index = iIndex * (int)ElementType.Max + (int)ElementType.Top;
					Angles[index] = (diff == 0) ? 0 : (float)Math.Atan(db * diff);
				}
			}
			Update01235678(pixcelDataGray16, iX, iY);
		}
		private void Update0123(ushort[] pixcelDataGray16, int iX, int iY, int index, double db, double dh)
		{
			var isXMin = iX == 0;
			var isXMax = iX == (Width - 1);
			var isYMin = iY == 0;

			var lc = iY * Width + iX;
			var lu = lc - Width;
			var v4 = pixcelDataGray16[lc];
			if(v4 != 0)
			{
				var diff0 = (isYMin | isXMin) ? 0 : (pixcelDataGray16[lu - 1] - v4);
				var diff1 = (isYMin)          ? 0 : (pixcelDataGray16[lu    ] - v4);
				var diff2 = (isYMin | isXMax) ? 0 : (pixcelDataGray16[lu + 1] - v4);
				var diff3 = (isXMin)          ? 0 : (pixcelDataGray16[lc - 1] - v4);

				Angles[index++] = (diff0 == 0) ? 0 : (float)Math.Atan(dh * diff0);
				Angles[index++] = (diff1 == 0) ? 0 : (float)Math.Atan(db * diff1);
				Angles[index++] = (diff2 == 0) ? 0 : (float)Math.Atan(dh * diff2);
				Angles[index++] = (diff3 == 0) ? 0 : (float)Math.Atan(db * diff3);
			}
			else
			{
				Angles[index++] = 0;
				Angles[index++] = 0;
				Angles[index++] = 0;
				Angles[index++] = 0;
			}
		}
		private void Update8765(int iX, int iY, int index)
		{
			var isXMin = iX == 0;
			var isXMax = iX == (Width - 1);
			var isYMax = iY == (Height - 1);

			var lc = iY * Width + iX;
			var ld = lc + Width;

			Angles[index++] = (isYMax | isXMax) ? 0 : -Angles[(ld + 1) * (int)ElementType.Max];
			Angles[index++] = (isYMax)          ? 0 : -Angles[(ld    ) * (int)ElementType.Max + 1];
			Angles[index++] = (isYMax | isXMin) ? 0 : -Angles[(ld - 1) * (int)ElementType.Max + 2];
			Angles[index++] = (isXMax)          ? 0 : -Angles[(lc + 1) * (int)ElementType.Max + 3];
		}
		private void Update01235678(ushort[] pixcelDataGray16, int iX, int iY)
		{
			var db = 0.1 / PixelDistance;
			var dh = db / Math.Sqrt(2);
			var index = (iX + iY * Width) * (int)ElementType.Max;
			Update0123(pixcelDataGray16, iX, iY, index, db, dh);
			Update8765(iX, iY, index + 4);
		}
	}
}
