using ImageMagick;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GmlConverter.Utilities
{
	internal static class BitmapSourceHelper
	{
		/// <summary>
		/// 16bit grayscale を自前で g:上位 b:下位 に変換し、BitmapSource.Create() で生成する。
		/// Magick.Net で可能な方法を幾つか試したが、 gamma の補正が入ってしまうため、自力で変換している。
		/// </summary>
		/// <param name="inputImage"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		internal static BitmapSource ToBitmapSourceFrom16bitGrayscaleToBgr(MagickImage inputImage, string? outputFileName = null)
		{
			using var pixels = inputImage.GetPixelsUnsafe();
			var pixcelDataGray16 = pixels.ToArray();
			if (pixcelDataGray16 == null)
			{
				throw new Exception();
			}
			var width = inputImage.Width;
			var height = inputImage.Height;
			int strideBgr = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
			byte[] pixcelDataBgr = ToBgrArrayFrom16bitGrayscaleArray(pixcelDataGray16, width, height);
			

			//内容を確認するための出力処理。
			if (outputFileName != null)
			{
				MagickReadSettings mrs = new()
				{
					Width = width,
					Height = height,
					ColorSpace = ColorSpace.RGB,
					ColorType = ColorType.Undefined,
					Depth = 8,
					Format = MagickFormat.Bgr,
				};
				using MagickImage mi = new(pixcelDataBgr, mrs);
				mi.Write(outputFileName);
			}

			return BitmapSource.Create(
				width, height, 96, 96,
				PixelFormats.Bgr24,
				null,
				pixcelDataBgr, strideBgr);
		}

		internal static byte[] ToBgrArrayFrom16bitGrayscaleArray(ushort[] pixcelDataGray16, int width, int height)
		{
			//grayscale bmp の作成
			int strideBgr = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
			byte[] pixcelDataBgr = new byte[height * strideBgr];

			int offsetGray16 = 0;
			for (int y = 0; y < height; ++y)
			{
				int offsetBgr = strideBgr * y;
				for (int x = 0; x < width; ++x)
				{
					var h = pixcelDataGray16[offsetGray16++];
					pixcelDataBgr[offsetBgr++] = (byte)(h & 0xff);
					pixcelDataBgr[offsetBgr++] = (byte)((h & 0xff00) >> 8);
					pixcelDataBgr[offsetBgr++] = 0;
				}
			}
			return pixcelDataBgr;
		}
	}
}