using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GmlConverter.Models.Gml
{
	/// <summary>
	/// GML ファイルを扱う際に使用する定数関連を定義したクラス
	/// </summary>
	internal static class GmlHelpers
	{
		/// <summary>
		/// Gml のスキーマの xsd ファイルのパスです。
		/// </summary>
		internal static string XsdPath = "FGD_GMLSchema.xsd";

		/// <summary>
		/// 標高の最低値。青森県の八戸石灰鉱山あたりが -170 m なのでそれより低く設定
		/// </summary>
		internal static double MinHeight = -200.0;//0.0;

		/// <summary>
		/// 標高の最高値。富士山が 3776 m なのでそれより高く設定
		/// </summary>
		internal static double MaxHeight = 3780.0;//3780.0;

		/// <summary>
		/// 海に設定する標高。 0 より少し低くしたほうが見分けがつくので良い。
		/// </summary>
		internal static double SeaHeight = 0.0;

		/// <summary>
		/// データが設定されていないなどのときに使用される標高。
		/// </summary>
		internal static double ErrorHeight = -9999.0;

		/// <summary>
		/// 標高マップの基準点
		/// </summary>
		internal static double HeightmapOrigin = -500.0;//3780.0;

		/// <summary>
		/// Heightmap 、これは海の色。
		/// </summary>
		internal static System.Drawing.Color SeaColor = System.Drawing.Color.FromArgb(0x00, System.Drawing.Color.Black);    //System.Drawing.Color.FromArgb(0x7f, System.Drawing.Color.Aqua);

		/// <summary>
		/// Heightmap 、これはエラーの色。
		/// </summary>
		internal static System.Drawing.Color ErrorColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);

		internal static double GrayscaleToHight(ushort grayscale)
		{
			return grayscale / 10.0 + HeightmapOrigin;
		}
		/// <summary>
		/// 高度から Grayscale の色を取得
		/// </summary>
		/// <param name="height">高度</param>
		/// <returns></returns>
		internal static ushort HightToGrayscale(double height)
		{
			return (ushort)((Math.Clamp(height, MinHeight, MaxHeight) - HeightmapOrigin) * 10);
		}

		/// <summary>
		/// 高度から高度マップ用画像の色を取得する。
		/// </summary>
		/// <param name="height">r は少数第1位と2位、g は一の位と十の位、b は百の位と千の位</param>
		/// <returns></returns>
		internal static System.Drawing.Color HightToHeightmapColor(double height)
		{
			var heightx10 = HightToGrayscale(height);

			// r は少数第1位と2位、g は一の位と十の位、b は百の位と千の位
			//var r = heightx100 % 100;
			//var g = (heightx100 / 100) % 100;
			//var b = heightx100 / 10000;

			// b は 1 byte 目、g は 2 byte 目
			var b = heightx10 & 0xff;
			var g = heightx10 >> 8 & 0xff;

			return System.Drawing.Color.FromArgb(0, g, b);
		}

		/// <summary>
		/// 画像用ピクセルバッファの構築
		/// </summary>
		/// <param name="pixelFormat">ピクセルバッファのフォーマット</param>
		/// <param name="setColor">色の設定方法</param>
		/// <param name="getColor">色の取得豊富</param>
		/// <param name="gridDivisions">ピクセルバッファのサイズ</param>
		/// <param name="cancellationToken">キャンセル用オブジェクト</param>
		/// <returns></returns>
		internal static BitmapSource CreateBitmap(PixelFormat pixelFormat, BitmapPalette? palette, Action<byte[], int, System.Drawing.Color> setColor, Func<int, int, System.Drawing.Color?> getColor, System.Drawing.Size gridDivisions, CancellationToken cancellationToken)
		{
			var mx = gridDivisions.Width;
			var my = gridDivisions.Height;
			var rawStride = (mx * pixelFormat.BitsPerPixel + 7) / 8;
			var pixcels = new byte[my * rawStride];
			var bytePerPixel = pixelFormat.BitsPerPixel / 8;

			for (int y = 0; y < my; ++y)
			{
				var previewOffset = y * rawStride;
				for (int x = 0; x < mx; ++x)
				{
					var color = getColor(x, y);
					if (color != null)
					{
						setColor(pixcels, previewOffset, color.Value);
					}
					previewOffset += bytePerPixel;
				}
				cancellationToken.ThrowIfCancellationRequested();
			}

			// ピクセルデータから画像を生成
			var bitmap = BitmapSource.Create(
				gridDivisions.Width, gridDivisions.Height, 96, 96, pixelFormat, palette,
				pixcels, rawStride);

			//UI スレッドで使えるように Freeze する
			bitmap.Freeze();

			return bitmap;
		}

		/// <summary>
		/// ビットマップの生成の設定
		/// </summary>
		internal class BitmapSetting
		{
			/// <summary>
			/// 画像で使用するピクセルフォーマット
			/// </summary>
			internal PixelFormat PixelFormat;

			/// <summary>
			/// 画像で使用するピクセルフォーマット
			/// </summary>
			internal BitmapPalette? Palette;

			/// <summary>
			/// pixcel バッファへ色情報のコピー
			/// </summary>
			internal Action<byte[], int, System.Drawing.Color> CopyPixels;

			/// <summary>
			/// 画像用ピクセルバッファの構築
			/// </summary>
			/// <param name="getColor">色の取得豊富</param>
			/// <param name="gridDivisions">ピクセルバッファのサイズ</param>
			/// <param name="cancellationToken">キャンセル用オブジェクト</param>
			/// <returns></returns>
			internal BitmapSource CreateBitmap(Func<int, int, System.Drawing.Color?> getColor, System.Drawing.Size gridDivisions, CancellationToken cancellationToken) =>
				GmlHelpers.CreateBitmap(PixelFormat, Palette, CopyPixels, getColor, gridDivisions, cancellationToken);


			internal BitmapSetting(PixelFormat pixcelFormat, BitmapPalette? palette, Action<byte[], int, System.Drawing.Color> copyPixels)
			{
				PixelFormat = pixcelFormat;
				Palette = palette;
				CopyPixels = copyPixels;
			}
		}

		/// <summary>
		/// Heightmap 用のビットマップの設定
		/// </summary>
		internal static BitmapSetting s_BitmapSetingHeightmap = new(
			PixelFormats.Bgra32,
			null,
			(pixels, offset, color) =>
			{
				pixels[offset + 0] = color.B;
				pixels[offset + 1] = color.G;
				pixels[offset + 2] = color.R;
				pixels[offset + 3] = color.A;
			});

		/// <summary>
		/// ピクセル間距離からメッシュサイズを取得する
		/// </summary>
		/// <param name="PixelDistance">ピクセル間距離</param>
		/// <returns>メッシュサイズ</returns>
		internal static System.Drawing.Size GetMesh2Size(int PixelDistance)
			=> PixelDistance switch { 1 => new(11250, 7500), 5 => new(2250, 1500), 10 => new(1125, 750), _ => new(0, 0) };
		internal static System.Drawing.Size GetMesh3Size(int PixelDistance)
			=> PixelDistance switch { 1 => new(1125, 750), 5 => new(225, 150), 10 => new(0, 0), _ => new(0, 0) };
	}
}
