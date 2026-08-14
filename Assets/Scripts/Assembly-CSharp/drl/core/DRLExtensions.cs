using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.core
{
	public static class DRLExtensions
	{
		public static void Shuffle<T>(this IList<T> list)
		{
			if (list == null)
			{
				Debug.LogError("DRLExtensions>Shuffle - list to shuffle can't be null");
				return;
			}
			System.Random random = new System.Random();
			int num = list.Count;
			while (num > 1)
			{
				num--;
				int index = random.Next(num + 1);
				T value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
		}
	}
}
