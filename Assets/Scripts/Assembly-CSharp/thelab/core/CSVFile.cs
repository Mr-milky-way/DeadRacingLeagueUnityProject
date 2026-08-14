using System;
using System.Collections.Generic;

namespace thelab.core
{
	public class CSVFile
	{
		public string source { get; protected set; }

		public string[][] lines { get; protected set; }

		public int length => lines.Length - 1;

		public string[] tokens { get; protected set; }

		public string[] keys { get; protected set; }

		public CSVFile(string p_source)
		{
			source = p_source;
			string[] array = source.Split('\n');
			lines = new string[array.Length][];
			List<string> list = new List<string>();
			for (int i = 0; i < lines.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				lines[i] = array2;
				list.AddRange(array2);
			}
			keys = new string[lines[0].Length];
			for (int j = 0; j < keys.Length; j++)
			{
				keys[j] = lines[0][j].Trim();
			}
			tokens = list.ToArray();
		}

		public T Get<T>(int p_line, string p_key)
		{
			return Get(p_line, p_key, default(T));
		}

		public T Get<T>(int p_line, string p_key, T p_default)
		{
			int num = p_line + 1;
			if (num <= 0)
			{
				return p_default;
			}
			if (num >= lines.Length)
			{
				return p_default;
			}
			int num2 = Array.IndexOf(keys, p_key);
			if (num2 < 0)
			{
				return p_default;
			}
			if (num2 >= lines[num].Length)
			{
				return p_default;
			}
			return Parse(lines[num][num2], p_default);
		}

		public T GetToken<T>(int p_id)
		{
			return GetToken(p_id, default(T));
		}

		public T GetToken<T>(int p_id, T p_default)
		{
			if (p_id < 0)
			{
				return p_default;
			}
			if (p_id >= tokens.Length)
			{
				return p_default;
			}
			return Parse(tokens[p_id], p_default);
		}

		public T Parse<T>(string p_value)
		{
			return Parse(p_value, default(T));
		}

		public T Parse<T>(string p_value, T p_default)
		{
			if (typeof(T) == typeof(string))
			{
				return (T)(object)p_value;
			}
			if (typeof(T) == typeof(int))
			{
				int result = 0;
				if (!int.TryParse(p_value, out result))
				{
					result = (int)(object)p_default;
				}
				return (T)(object)result;
			}
			if (typeof(T) == typeof(float))
			{
				float result2 = 0f;
				if (!float.TryParse(p_value, out result2))
				{
					return p_default;
				}
				return (T)(object)result2;
			}
			if (typeof(T) == typeof(bool))
			{
				return (T)(object)(p_value.ToLower() == "true");
			}
			return p_default;
		}
	}
}
