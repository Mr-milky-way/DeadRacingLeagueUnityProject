using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class ColliderEvent
	{
		[Flags]
		public enum Type : byte
		{
			Enter = 1,
			Exit = 2,
			Stay = 4,
			All = 7
		}

		public Type type;

		public ColliderEventComponent target;

		public bool trigger;

		public Collider collider;

		public Collision data;

		public Vector3 hitEnter;

		public Vector3 hitExit;
	}
}
