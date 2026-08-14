using System;

namespace drl
{
	[Serializable]
	public class AssetActionEvent
	{
		public MapAssetAction target;

		public object data;
	}
}
