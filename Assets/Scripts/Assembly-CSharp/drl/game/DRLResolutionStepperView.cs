using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class DRLResolutionStepperView : DRLListStepperView<Vector2>
	{
		public int minWidth;

		public int minHeight;

		protected override void Awake()
		{
			List<Resolution> list = new List<Resolution>(Screen.resolutions);
			if (Application.isEditor)
			{
				list.Clear();
				list.Add(Screen.currentResolution);
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].width < minWidth)
				{
					list.RemoveAt(i--);
				}
				else if (list[i].height < minHeight)
				{
					list.RemoveAt(i--);
				}
			}
			List<string> list2 = new List<string>();
			List<Vector2> list3 = new List<Vector2>();
			list.Sort((Resolution a, Resolution b) => (a.width <= b.width) ? 1 : (-1));
			int num = 0;
			int num2 = 0;
			foreach (Resolution item2 in list)
			{
				if (num != item2.width || num2 != item2.height)
				{
					string item = item2.width.ToString("0") + "x" + item2.height.ToString("0");
					Vector2 vector = new Vector2(item2.width, item2.height);
					list2.Add(item);
					if ((value - vector).magnitude <= 0f)
					{
						index = list3.Count;
					}
					list3.Add(vector);
					num = item2.width;
					num2 = item2.height;
				}
			}
			labels = list2.ToArray();
			values.Clear();
			values.AddRange(list3);
			min = 0;
			max = list2.Count - 1;
			base.Awake();
		}

		public Vector2 GetClosestResolution()
		{
			Vector2 result = value;
			List<Resolution> list = new List<Resolution>(Screen.resolutions);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].width < minWidth)
				{
					list.RemoveAt(i--);
				}
				else if (list[i].height < minHeight)
				{
					list.RemoveAt(i--);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				Resolution resolution = list[j];
				if (!(Mathf.Abs((float)resolution.width - result.x) > 0f) && !(Mathf.Abs((float)resolution.height - result.y) > 0f))
				{
					return result;
				}
			}
			result.x = Screen.currentResolution.width;
			result.y = Screen.currentResolution.height;
			return result;
		}

		public void SetClosestResolution(Vector2 s)
		{
			value = s;
			for (int i = 0; i < values.Count; i++)
			{
				Vector2 vector = values[i];
				if (!(Mathf.Abs(vector.x - s.x) > 0f) && !(Mathf.Abs(vector.y - s.y) > 0f))
				{
					index = i;
					Refresh();
				}
			}
		}
	}
}
