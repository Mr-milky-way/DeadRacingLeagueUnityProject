using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class DRLResolutionDropdownView : DRLDropdownView
	{
		private int i;

		[Tooltip("Minimum allowed resolution width.")]
		public int minWidth = 800;

		[Tooltip("Minimum allowed resolution height.")]
		public int minHeight = 600;

		protected override void Awake()
		{
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
			List<string> list2 = new List<string>();
			List<Vector2> list3 = new List<Vector2>();
			list.Sort((Resolution a, Resolution b) => (a.height <= b.height) ? 1 : (-1));
			int num = 0;
			int num2 = 0;
			foreach (Resolution item3 in list)
			{
				if (num != item3.width || num2 != item3.height)
				{
					string item = item3.width.ToString("0") + "x" + item3.height.ToString("0");
					Vector2 item2 = new Vector2(item3.width, item3.height);
					list2.Add(item);
					list3.Add(item2);
					num = item3.width;
					num2 = item3.height;
				}
			}
			Set(list2.Distinct().ToList());
			base.Awake();
		}

		protected override void OnOpen()
		{
			base.OnOpen();
			Transform transform = base.dropdown.transform.Find("Dropdown List");
			if (!(transform != null))
			{
				return;
			}
			RectTransform content = transform.GetComponent<ScrollRect>().content;
			int num = 0;
			foreach (Transform item in content)
			{
				if (item.gameObject.activeSelf)
				{
					item.Find("aspect-ratio").GetComponent<Text>().text = GetAspectRatio(GetResolution(num));
					num++;
				}
			}
		}

		private string GetAspectRatio(Vector2 res)
		{
			float num = res.x / res.y;
			if (num >= 2.05f)
			{
				return "21:10";
			}
			if (num >= 1.7f)
			{
				return "16:9";
			}
			if (num >= 1.59f)
			{
				return "16:10";
			}
			if (num >= 1.489f)
			{
				return "3:2";
			}
			if (num >= 1.32f)
			{
				return "4:3";
			}
			if (num >= 1.23f)
			{
				return "5:4";
			}
			return "16:9";
		}

		public Vector2 GetClosestResolution()
		{
			Vector2 resolution = GetResolution();
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
				Resolution resolution2 = list[j];
				if (!(Mathf.Abs((float)resolution2.width - resolution.x) > 0f) && !(Mathf.Abs((float)resolution2.height - resolution.y) > 0f))
				{
					return resolution;
				}
			}
			resolution.x = Screen.currentResolution.width;
			resolution.y = Screen.currentResolution.height;
			return resolution;
		}

		public void SetClosestResolution(Vector2 s)
		{
			if (SetResolution(s))
			{
				return;
			}
			for (int i = 0; i < options.Count; i++)
			{
				Vector2 resolution = GetResolution(i);
				if (!(Mathf.Abs(resolution.x - s.x) > 0f) && !(Mathf.Abs(resolution.y - s.y) > 0f))
				{
					SetResolution(resolution);
				}
			}
		}

		public Vector2 GetResolution(int p_idx = -1)
		{
			string text = ((p_idx < 0 || p_idx >= options.Count) ? Value().text : options[p_idx].text);
			int num = 0;
			int num2 = 0;
			if (string.IsNullOrEmpty(text))
			{
				return Vector2.zero;
			}
			string[] array = text.Split(' ')[0].Split('x');
			if (array.Length != 2)
			{
				return Vector2.zero;
			}
			try
			{
				num = int.Parse(array[0]);
				num2 = int.Parse(array[1]);
			}
			catch (InvalidCastException ex)
			{
				Debug.LogError("DRLResolutionDropdown>>" + ex.Message);
			}
			return new Vector2(num, num2);
		}

		private bool SetResolution(Vector2 p_res)
		{
			bool result = false;
			int num = (int)p_res[0];
			int num2 = (int)p_res[1];
			string text = num.ToString() + 'x' + num2.ToString();
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].text == text)
				{
					Select(i);
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
