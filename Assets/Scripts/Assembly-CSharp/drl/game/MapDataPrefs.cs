using System;
using thelab.core;

namespace drl.game
{
	[Serializable]
	public class MapDataPrefs : SerializedData
	{
		public bool autoSave
		{
			get
			{
				return Get("map-prefs-auto-save", d: true);
			}
			set
			{
				Set("map-prefs-auto-save", value);
			}
		}
	}
}
