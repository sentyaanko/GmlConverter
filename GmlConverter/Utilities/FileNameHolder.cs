using System.Text.RegularExpressions;

namespace GmlConverter.Utilities
{
    internal class FileNameHolder
    {
        public string FilePath { get; private set; } = string.Empty;
		public string DirectoryName { get; private set; } = string.Empty;
		public string FileName { get; private set; } = string.Empty;
		public string FileNameWithoutExtension { get; private set; } = string.Empty;
		public string Extension { get; private set; } = string.Empty;

		public string FileNameWithoutExtensionAndPixelDistance { get; private set; } = string.Empty;
		public int PixelDistance { get; private set; } = 0;

		internal FileNameHolder()
        {
		}
		internal FileNameHolder(string filePath)
        {
			FilePath = filePath;
			DirectoryName = System.IO.Path.GetDirectoryName(filePath) ?? string.Empty;
			FileName = System.IO.Path.GetFileName(filePath);
			FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
			Extension = System.IO.Path.GetExtension(filePath) ?? string.Empty;

			var pattern = @"(.*)-(1|5|10)\.png";
			var match = Regex.Match(FileName, pattern);
			if (match.Success)
			{
				FileNameWithoutExtensionAndPixelDistance = match.Groups[1].Value;
				PixelDistance = int.Parse(match.Groups[2].Value);
			}
            else
			{
				FileNameWithoutExtensionAndPixelDistance = FileNameWithoutExtension;
				PixelDistance = 0;
			}
		}
		internal string GetRelatedFilesWildCard()
		{
			return (PixelDistance == 0)
				? $"{FileNameWithoutExtensionAndPixelDistance}*{Extension}"
				: $"{FileNameWithoutExtensionAndPixelDistance}*-{PixelDistance}{Extension}";
		}
		internal string CreateRelatedFileName(string add)
		{
			return (PixelDistance == 0)
				? $"{FileNameWithoutExtensionAndPixelDistance}-{add}{Extension}"
				: $"{FileNameWithoutExtensionAndPixelDistance}-{add}-{PixelDistance}{Extension}";
		}
	}
}

