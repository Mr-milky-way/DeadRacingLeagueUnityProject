using thelab.core;

namespace drl.backend
{
	public class DRLProgressionTrackData : SerializedData
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

		public string name
		{
			get
			{
				return Get("name", "");
			}
			set
			{
				Set("name", value);
			}
		}

		public int xp
		{
			get
			{
				return Get("xp-value", 0);
			}
			set
			{
				Set("xp-value", value);
			}
		}

		public int minTime
		{
			get
			{
				return Get("xp-min-time", 0);
			}
			set
			{
				Set("xp-min-time", value);
			}
		}
	}
}
