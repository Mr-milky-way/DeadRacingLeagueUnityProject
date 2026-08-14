using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	[ExecuteInEditMode]
	public class CubeLineRenderer : MonoBehaviour
	{
		private void Awake()
		{
			float num = 0.5f;
			Vector3[] array = new Vector3[8]
			{
				new Vector3(0f - num, num, 0f - num),
				new Vector3(num, num, 0f - num),
				new Vector3(num, num, num),
				new Vector3(0f - num, num, num),
				new Vector3(0f - num, 0f - num, 0f - num),
				new Vector3(num, 0f - num, 0f - num),
				new Vector3(num, 0f - num, num),
				new Vector3(0f - num, 0f - num, num)
			};
			int[] array2 = new int[18]
			{
				0, 4, 5, 1, 2, 6, 7, 3, 0, 4,
				0, 1, 5, 6, 2, 3, 7, 4
			};
			LineRenderer component = GetComponent<LineRenderer>();
			if ((bool)component)
			{
				List<Vector3> list = new List<Vector3>();
				for (int i = 0; i < array2.Length; i++)
				{
					list.Add(array[array2[i]]);
				}
				component.positionCount = list.Count;
				component.SetPositions(list.ToArray());
			}
		}
	}
}
