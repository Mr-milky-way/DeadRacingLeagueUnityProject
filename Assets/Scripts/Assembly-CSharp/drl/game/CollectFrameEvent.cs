using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public struct CollectFrameEvent
	{
		public int index;

		public float time;

		public Vector3 position;
	}
}
