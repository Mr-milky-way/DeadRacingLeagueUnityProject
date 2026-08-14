using thelab.core;

namespace drl.backend
{
	public class DRLLoginData : SerializedData
	{
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

		public string uid
		{
			get
			{
				return Get("uid", "");
			}
			set
			{
				Set("uid", value);
			}
		}

		public string version
		{
			get
			{
				return Get("version", "");
			}
			set
			{
				Set("version", value);
			}
		}

		public string time
		{
			get
			{
				return Get("time", "");
			}
			set
			{
				Set("time", value);
			}
		}

		public string checksum
		{
			get
			{
				return Get("checksum", "");
			}
			set
			{
				Set("checksum", value);
			}
		}
	}
}
