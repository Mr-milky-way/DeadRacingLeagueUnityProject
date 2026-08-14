using System;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public struct LayoutSlice
	{
		[Range(0f, 1f)]
		public float x;

		[Range(0f, 1f)]
		public float y;

		[Range(0f, 1f)]
		public float z;

		[Range(0f, 1f)]
		public float rangeX;

		[Range(0f, 1f)]
		public float rangeY;

		[Range(0f, 1f)]
		public float rangeZ;

		public void Set(params float[] p_values)
		{
			int num = 0;
			x = p_values[num++];
			y = p_values[num++];
			z = p_values[num++];
			rangeX = p_values[num++];
			rangeY = p_values[num++];
			rangeZ = p_values[num++];
		}
	}
}
