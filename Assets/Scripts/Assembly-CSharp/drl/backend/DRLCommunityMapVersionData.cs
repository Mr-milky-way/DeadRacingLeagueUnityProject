using thelab.core;

namespace drl.backend
{
	public class DRLCommunityMapVersionData : SerializedData
	{
		public string guid
		{
			get
			{
				return Get("guid", "");
			}
			set
			{
				Set("guid", value);
			}
		}

		public int version
		{
			get
			{
				return Get("version", -1);
			}
			set
			{
				Set("version", value);
			}
		}

		public DRLCommunityMapVersionData(string p_guid, int p_version)
		{
			guid = p_guid;
			version = p_version;
		}
	}
}
