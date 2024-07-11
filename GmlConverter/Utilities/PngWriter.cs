using ImageMagick;

namespace GmlConverter.Utilities
{
    internal static class PngWriter
    {
        internal static void WritePng(string fileName, int width, int height, byte[] data)
        {
            MagickReadSettings mrs = new()
            {
                Width = width,
                Height = height,
                ColorSpace = ColorSpace.Gray,
                Depth = 16,
                Format = MagickFormat.Gray,
            };
            using MagickImage mi = new(data, mrs);
            WritePng(fileName, mi);
        }
		internal static void WritePng(string fileName, MagickImage mi)
		{
            mi.Format = MagickFormat.Png00;
			mi.Settings.SetDefine("png:color-type", "0");
			mi.Settings.SetDefine("png:bit-depth", "16");
			mi.Write(fileName);
		}
	}
}