using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public struct LayoutParams
	{
		public float span;

		public LayoutSlice slices;

		public int seed;

		public Vector3 random;

		public bool fill;

		public int max;

		public bool dynamic => max > 0;
	}
}
