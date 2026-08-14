using System;
using UnityEngine.Events;

namespace drl.backend
{
	[Serializable]
	public class SteamEvent : UnityEvent<SteamEventData>
	{
	}
}
