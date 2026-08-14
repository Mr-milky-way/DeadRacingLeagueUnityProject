using thelab.core;

namespace drl.backend
{
	public class DRLCircuitMapData : SerializedData
	{
		public string mapId => Get<string>("map-id");

		public string trackId => Get<string>("track-id");

		public bool isCustom => Get("is-custom", d: false);
	}
}
