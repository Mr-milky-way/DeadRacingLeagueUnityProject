using thelab.core;

namespace drl.backend
{
	public class DRLMapFavoriteData : SerializedData
	{
		public string mapId
		{
			get
			{
				return Get("map-id", "");
			}
			set
			{
				Set("map-id", value);
			}
		}

		public string trackId
		{
			get
			{
				return Get("track-id", "");
			}
			set
			{
				Set("track-id", value);
			}
		}

		public bool customMap
		{
			get
			{
				return Get("custom-map", d: false);
			}
			set
			{
				Set("custom-map", value);
			}
		}
	}
}
