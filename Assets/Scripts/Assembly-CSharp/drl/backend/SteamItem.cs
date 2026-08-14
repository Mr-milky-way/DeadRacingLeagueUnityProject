using System;

namespace drl.backend
{
	[Serializable]
	public class SteamItem
	{
		public string name;

		public string appid;

		public string Timestamp;

		public string modified;

		public string itemdefid;

		public string itemid;

		public string guid;

		public string type;

		public string display_type;

		public string bundle;

		public string name_color;

		public string background_color;

		public string item_slot;

		public string item_quality;

		public string icon_url;

		public string icon_url_large;

		public int quantity;

		public int flags;

		public string description;

		public string hash;

		public bool tradable;

		public bool marketable;

		public bool commodity;

		public bool store_hidden;

		public string price_category;

		public string price;

		public float priceVLV;

		public int drop_interval;

		public int drop_max_per_window;

		public string workshopid;

		public string properties;
	}
}
