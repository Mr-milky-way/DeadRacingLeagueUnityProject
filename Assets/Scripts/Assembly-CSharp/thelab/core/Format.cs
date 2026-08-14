using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace thelab.core
{
	public class Format
	{
		private static StringBuilder sttb = new StringBuilder();

		private static string[] nsc00;

		private static string[] nsc;

		private static string formatString;

		public static string SecondsToTime(float p_seconds, int p_format_places = 2, bool p_use_ms = false, string p_separator = ":")
		{
			int n = (p_use_ms ? (Mathf.FloorToInt(p_seconds * 100f) % 100) : 0);
			int n2 = Mathf.FloorToInt(p_seconds) % 60;
			int n3 = ((p_format_places >= 2) ? (Mathf.FloorToInt(p_seconds / 60f) % 60) : 0);
			int n4 = ((p_format_places >= 3) ? (Mathf.FloorToInt(p_seconds / 60f / 60f) % 24) : 0);
			int n5 = ((p_format_places >= 4) ? Mathf.FloorToInt(p_seconds / 60f / 60f / 24f) : 0);
			sttb.Length = 0;
			if (p_format_places >= 4)
			{
				sttb.Append(IntToString(n5));
			}
			if (p_format_places >= 3)
			{
				if (p_format_places >= 4)
				{
					sttb.Append(p_separator);
				}
				sttb.Append(IntToString(n4));
			}
			if (p_format_places >= 2)
			{
				if (p_format_places >= 3)
				{
					sttb.Append(p_separator);
				}
				sttb.Append(IntToString(n3));
			}
			if (p_format_places >= 2)
			{
				sttb.Append(p_separator);
			}
			sttb.Append(IntToString(n2));
			if (p_use_ms)
			{
				sttb.Append(p_separator);
				sttb.Append(IntToString(n));
			}
			return sttb.ToString();
		}

		public static string IntToString(int n, bool p_lead_zero = true)
		{
			if (nsc00 == null)
			{
				nsc00 = new string[100];
				for (int i = 0; i < nsc00.Length; i++)
				{
					nsc00[i] = i.ToString("00");
				}
				nsc = new string[100];
				for (int j = 0; j < nsc.Length; j++)
				{
					nsc[j] = j.ToString();
				}
			}
			if (!p_lead_zero)
			{
				return nsc[n];
			}
			return nsc00[n];
		}

		public static string SecondsToMMSSFFF(float p_seconds)
		{
			formatString = TimeSpan.FromSeconds(p_seconds).ToString("mm\\:ss\\.fff");
			return formatString;
		}

		public static string DateHash(string p_yf = "yyyy", string p_mf = "MM", string p_df = "dd", string p_thf = "HH", string p_tmf = "mm", string p_tsf = "ss", string p_separator = "")
		{
			return DateHash(DateTime.UtcNow, p_yf, p_mf, p_df, p_thf, p_tmf, p_tsf, p_separator);
		}

		public static string DateHash(DateTime p_date, string p_yf = "yyyy", string p_mf = "MM", string p_df = "dd", string p_thf = "HH", string p_tmf = "mm", string p_tsf = "ss", string p_separator = "")
		{
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(p_yf))
			{
				list.Add(p_date.ToString(p_yf));
			}
			if (!string.IsNullOrEmpty(p_mf))
			{
				list.Add(p_date.ToString(p_mf));
			}
			if (!string.IsNullOrEmpty(p_df))
			{
				list.Add(p_date.ToString(p_df));
			}
			if (!string.IsNullOrEmpty(p_thf))
			{
				list.Add(p_date.ToString(p_thf));
			}
			if (!string.IsNullOrEmpty(p_tmf))
			{
				list.Add(p_date.ToString(p_tmf));
			}
			if (!string.IsNullOrEmpty(p_tsf))
			{
				list.Add(p_date.ToString(p_tsf));
			}
			return string.Join(p_separator, list.ToArray());
		}

		public static string Ordinal(int p_number)
		{
			return (p_number % 24) switch
			{
				1 => "st", 
				2 => "nd", 
				3 => "rd", 
				21 => "st", 
				22 => "nd", 
				23 => "rd", 
				_ => "th", 
			};
		}

		public static string VersionTrimm(string p_v, int p_size = 2, char p_separator = '.')
		{
			if (string.IsNullOrEmpty(p_v))
			{
				p_v = "";
			}
			string[] array = p_v.Split(p_separator);
			if (array.Length <= p_size)
			{
				return p_v;
			}
			return string.Join(p_separator.ToString() ?? "", array, 0, p_size);
		}

		public static int VersionCompare(string p_v1, string p_v2, string p_separator = ".")
		{
			if (string.IsNullOrEmpty(p_v1))
			{
				p_v1 = "";
			}
			if (string.IsNullOrEmpty(p_v2))
			{
				p_v2 = "";
			}
			string[] array = p_v1.Split(p_separator[0]);
			string[] array2 = p_v2.Split(p_separator[0]);
			List<string> list = new List<string>((array.Length >= array2.Length) ? array : array2);
			List<string> list2 = new List<string>((array.Length < array2.Length) ? array : array2);
			while (list.Count > list2.Count)
			{
				list2.Add("0");
			}
			for (int i = 0; i < list.Count; i++)
			{
				string text = list[i];
				string text2 = list2[i];
				bool flag = true;
				int result = -1;
				if (!int.TryParse(text, out result))
				{
					flag = false;
				}
				int result2 = -1;
				if (!int.TryParse(text2, out result2))
				{
					flag = false;
				}
				if (flag)
				{
					if (result < result2)
					{
						return -1;
					}
					if (result > result2)
					{
						return 1;
					}
				}
				else
				{
					int num = string.Compare(text, text2);
					if (num != 0)
					{
						return num;
					}
				}
			}
			return 0;
		}

		public static string Join<T>(string p_separator, params T[] p_args)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < p_args.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(p_separator);
				}
				stringBuilder.Append(p_args[i].ToString());
			}
			return stringBuilder.ToString();
		}

		public static string MsToTime(int p_time, string p_format)
		{
			if (p_time < 0)
			{
				return "";
			}
			return new TimeSpan(0, 0, 0, 0, p_time).ToString(p_format);
		}

		public static string Join(string p_separator, params object[] p_args)
		{
			return Format.Join<object>(p_separator, p_args);
		}
	}
}
