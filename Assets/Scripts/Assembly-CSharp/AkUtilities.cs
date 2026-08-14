using System.IO;
using System.Text;

public class AkUtilities
{
	public class ShortIDGenerator
	{
		private const uint s_prime32 = 16777619u;

		private const uint s_offsetBasis32 = 2166136261u;

		private static byte s_hashSize;

		private static uint s_mask;

		public static byte HashSize
		{
			get
			{
				return s_hashSize;
			}
			set
			{
				s_hashSize = value;
				s_mask = (uint)((1 << (int)s_hashSize) - 1);
			}
		}

		static ShortIDGenerator()
		{
			HashSize = 32;
		}

		public static uint Compute(string in_name)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(in_name.ToLower());
			uint num = 2166136261u;
			for (int i = 0; i < bytes.Length; i++)
			{
				num *= 16777619;
				num ^= bytes[i];
			}
			if (s_hashSize == 32)
			{
				return num;
			}
			return (num >> (int)s_hashSize) ^ (num & s_mask);
		}
	}

	public static void FixSlashes(ref string path, char separatorChar, char badChar, bool addTrailingSlash)
	{
		if (!string.IsNullOrEmpty(path))
		{
			path = path.Trim().Replace(badChar, separatorChar).TrimStart('\\');
			if (addTrailingSlash && !path.EndsWith(separatorChar.ToString()))
			{
				path += separatorChar;
			}
		}
	}

	public static void FixSlashes(ref string path)
	{
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		char badChar = ((directorySeparatorChar == '\\') ? '/' : '\\');
		FixSlashes(ref path, directorySeparatorChar, badChar, addTrailingSlash: true);
	}
}
