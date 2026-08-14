using UnityEngine;

namespace drl.game
{
	public class MapAssetComponent : MonoBehaviour
	{
		internal virtual void OnEvent(MapAsset p_target, MapAssetEventType p_type)
		{
		}
	}
}
