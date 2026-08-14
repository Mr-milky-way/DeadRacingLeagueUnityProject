using System;
using UnityEngine;

namespace thelab.core
{
	public class GUID
	{
		private static System.Random m_generator;

		protected static Func<string> DictionaryGenerator(params string[] p_dictionary)
		{
			int len = p_dictionary.Length;
			return delegate
			{
				if (len <= 0)
				{
					return "";
				}
				int num = UnityEngine.Random.Range(0, len);
				return p_dictionary[num];
			};
		}

		protected static Func<string> RangeGenerator(int p_min, int p_max, string p_format)
		{
			return delegate
			{
				if (m_generator == null)
				{
					m_generator = new System.Random(DateTime.Now.Millisecond);
				}
				return m_generator.Next(p_min, p_max).ToString(p_format);
			};
		}

		public static string Create(int p_length, string p_separator, int p_group, params string[] p_dictionary)
		{
			return Create(p_length, p_separator, p_group, DictionaryGenerator(p_dictionary));
		}

		public static string Create(int p_length, string p_separator, int p_group, int p_min, int p_max, string p_format)
		{
			return Create(p_length, p_separator, p_group, RangeGenerator(p_min, p_max, p_format));
		}

		public static string Create(int p_length, string p_separator, int p_group, Func<string> p_generator)
		{
			string text = "";
			if (p_generator == null)
			{
				return text;
			}
			int num = 0;
			for (int i = 0; i < p_length; i++)
			{
				string text2 = p_generator();
				if (i > 0 && p_group > 0 && num % p_group == 0)
				{
					text += p_separator;
				}
				if (p_group > 0)
				{
					num = (num + 1) % p_group;
				}
				text += text2;
			}
			return text;
		}
	}
}
