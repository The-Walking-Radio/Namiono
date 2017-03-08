using System;
using System.IO;

namespace Namiono
{
	public static class FileSystem
	{
		public static void Read(string path, ref byte[] data, out int bytesRead,
			int count = 0, int index = 0)
		{
			var length = (count == 0 || count >= data.Length) ? data.Length : count;
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				fs.Seek(index, SeekOrigin.Begin);
				bytesRead = fs.Read(data, 0, length);
				fs.Close();
				fs.Dispose();
			}
		}

		public static bool Exist(string path, FSObject type = FSObject.File)
		{
			var b = false;
			switch (type)
			{
				case FSObject.File:
					b = File.Exists(path);
					break;
				case FSObject.Directory:
					b = Directory.Exists(path);
					break;
				default:
					b = false;
					break;
			}

			return b;
		}

		public static string ResolvePath(string path, string rootDir)
		{
			var p = path.Trim();

			if (p.StartsWith("/", StringComparison.InvariantCultureIgnoreCase) && p.Length > 3)
				p = p.Remove(0, 1);

			p = Path.Combine(rootDir, p);

			if (p.Contains("\\") && p.Contains("/"))
				p = p.Replace('\\', '/');

			return p.Trim();
		}

		public static void Write(string path, ref byte[] data, long offset = 0)
		{
			using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
			{
				fs.Seek(offset, SeekOrigin.Begin);
				fs.Write(data, 0, data.Length);
				fs.Flush();
				fs.Close();
			}
		}

		public static long Size(string path)
		{
			var size = 0L;
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				size = fs.Length;
				fs.Close();
				fs.Dispose();
			}

			return size;
		}
	}
}
