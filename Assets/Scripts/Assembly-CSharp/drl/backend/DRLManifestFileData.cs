using System;
using thelab.core;

namespace drl.backend
{
	public class DRLManifestFileData : SerializedData
	{
		public string id
		{
			get
			{
				return Get("id", "");
			}
			set
			{
				Set("id", value);
			}
		}

		public string lastModifiedString
		{
			get
			{
				return Get("last-modified", "");
			}
			set
			{
				Set("last-modified", value);
			}
		}

		public DateTime lastModified => DateTime.Parse(lastModifiedString);

		public string path
		{
			get
			{
				return Get("path", "");
			}
			set
			{
				Set("path", value);
			}
		}

		public string localPath
		{
			get
			{
				string text = path;
				text = text.Replace(branch, "");
				text = text.Replace(platform, "");
				text = text.Replace("///", "/");
				text = text.Replace("//", "/");
				if (text.StartsWith("/"))
				{
					text = text.Substring(1);
				}
				return text;
			}
		}

		public string branch
		{
			get
			{
				return Get("branch", "");
			}
			set
			{
				Set("branch", value);
			}
		}

		public string platform
		{
			get
			{
				return Get("platform", "");
			}
			set
			{
				Set("platform", value);
			}
		}

		public int version
		{
			get
			{
				return Get("version", 0);
			}
			set
			{
				Set("version", value);
			}
		}

		public string url
		{
			get
			{
				return Get("url", "");
			}
			set
			{
				Set("url", value);
			}
		}
	}
}
