using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace thelab.core
{
	public class Git
	{
		private static string m_path;

		private static string m_branch;

		private static string m_commit;

		public static string path
		{
			get
			{
				if (!string.IsNullOrEmpty(m_path))
				{
					return m_path;
				}
				string dataPath = Application.dataPath;
				char c = ((dataPath.IndexOf('/') >= 0) ? '/' : '\\');
				List<string> list = new List<string>(dataPath.Split(c));
				list.RemoveAt(list.Count - 1);
				while (list.Count > 0)
				{
					string text = string.Join(c.ToString(), list.ToArray());
					switch (Application.platform)
					{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						text += "/.git/";
						if (Directory.Exists(text))
						{
							return m_path = text;
						}
						break;
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						text += "/.git/";
						if (Directory.Exists(text))
						{
							return m_path = text;
						}
						break;
					}
					list.RemoveAt(list.Count - 1);
				}
				return m_path = "";
			}
		}

		public static string branch
		{
			get
			{
				if (!string.IsNullOrEmpty(m_branch))
				{
					return m_branch;
				}
				string value = path;
				if (string.IsNullOrEmpty(value))
				{
					return "";
				}
				string[] files = Directory.GetFiles(value, "HEAD");
				if (files.Length == 0)
				{
					return "";
				}
				string text = File.ReadAllText(files[0]);
				char c = ((text.IndexOf('/') >= 0) ? '/' : '\\');
				string[] array = text.Split(c);
				return m_branch = ((array.Length == 0) ? "" : array[array.Length - 1].Trim());
			}
		}

		public static string commit
		{
			get
			{
				if (!string.IsNullOrEmpty(m_commit))
				{
					return m_commit;
				}
				string text = path;
				if (string.IsNullOrEmpty(text))
				{
					return m_commit = "";
				}
				string text2 = "";
				string[] files = Directory.GetFiles(text, "FETCH_HEAD");
				if (files.Length != 0)
				{
					text2 = File.ReadAllText(files[0]);
					text2 = ((text2.Length <= 0) ? "unknown-head-data" : text2.Substring(0, Mathf.Min(text2.Length, 40)));
				}
				text += "refs/";
				text += "heads/";
				if (string.IsNullOrEmpty(branch))
				{
					return m_commit = text2;
				}
				text += branch;
				if (!File.Exists(text))
				{
					return m_commit = text2;
				}
				string text3 = File.ReadAllText(text);
				if (string.IsNullOrEmpty(text3))
				{
					return m_commit = text2;
				}
				text3 = text3.Trim();
				text3 = text3.Replace("\n", "");
				return m_commit = text3;
			}
		}
	}
}
