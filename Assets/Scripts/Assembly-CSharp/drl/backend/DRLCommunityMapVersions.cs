using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLCommunityMapVersions : SerializedData
	{
		public DRLCommunityMapVersionData[] data
		{
			get
			{
				object obj = Get("data", (object)new DRLCommunityMapVersionData[0]);
				if (obj is JArray)
				{
					obj = (obj as JArray).ToObject<DRLCommunityMapVersionData[]>();
				}
				return (DRLCommunityMapVersionData[])obj;
			}
			set
			{
				Set("data", value);
			}
		}

		public DRLCommunityMapVersions()
		{
			data = new DRLCommunityMapVersionData[0];
		}
	}
}
