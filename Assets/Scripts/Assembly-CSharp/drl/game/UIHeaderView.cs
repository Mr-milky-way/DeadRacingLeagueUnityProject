using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHeaderView : View<DRLApp>
	{
		public HorizontalLayoutGroup pathContainer;

		public List<Text> pathFields;

		public Color[] colors;

		public Text versionField;

		public RectTransform logoContainer;

		public RectTransform logoSeparator;

		public FadeComponent pathFade;

		public RectTransform lowSpecWarningContent;

		public float lowSpecWarningContentWidth = 340f;

		public float lowSpecWarningWidth
		{
			get
			{
				return lowSpecWarningContent.sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = lowSpecWarningContent.sizeDelta;
				sizeDelta.x = value;
				lowSpecWarningContent.sizeDelta = sizeDelta;
			}
		}

		protected void Awake()
		{
			pathFields = new List<Text>(pathContainer ? Hierarchy.FindAll<Text>(pathContainer.transform).ToArray() : new Text[0]);
		}

		public void SetDebug(bool p_flag)
		{
		}

		public void Clear()
		{
			for (int i = 0; i < pathFields.Count; i++)
			{
				pathFields[i].gameObject.SetActive(value: false);
				pathFields[i].text = "";
			}
		}

		public void Set(UIScreenManager p_manager)
		{
			Set(p_manager ? p_manager.path : "");
		}

		public void Refresh()
		{
			Set(base.app.view.ui.screens.manager);
		}

		public void Set(string p_path)
		{
			Clear();
			string[] array = p_path.Split('/');
			int num = Mathf.Min(pathFields.Count / 2, array.Length);
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				string text = array[i].ToUpper().Replace("\n", " ");
				Text text2 = pathFields[num2];
				text2.gameObject.SetActive(value: true);
				text2.text = text;
				Color p_to = ((i >= num - 1) ? colors[1] : colors[0]);
				Tween.Kill(text2);
				Tween.Add(text2, "color", p_to, 0.2f, Cubic.Out);
				num2++;
				if (i < num - 1)
				{
					pathFields[num2].gameObject.SetActive(value: true);
					pathFields[num2].text = ">";
					num2++;
					continue;
				}
				break;
			}
		}

		public void ShowLowSpecWarning(bool p_show, float p_delay)
		{
			if (!(lowSpecWarningContent == null) && (!p_show || (int)lowSpecWarningWidth <= 0) && (p_show || (int)lowSpecWarningWidth >= (int)lowSpecWarningContentWidth - 2))
			{
				Tween.Kill(this, "lowSpecWarningWidth");
				if (p_show)
				{
					lowSpecWarningContent.transform.parent.gameObject.SetActive(value: true);
					lowSpecWarningWidth = 0f;
					Tween.Add(this, "lowSpecWarningWidth", lowSpecWarningContentWidth, 0.4f, p_delay, Cubic.Out);
				}
				else
				{
					lowSpecWarningWidth = lowSpecWarningContentWidth;
					Tween.Add(this, "lowSpecWarningWidth", 0f, 0.4f, p_delay, Cubic.Out);
				}
			}
		}

		public void FadeLogo(bool p_flag, float p_duration, float p_delay = 0f)
		{
			if ((bool)logoContainer && (bool)logoSeparator)
			{
				float num = p_delay;
				if (p_flag)
				{
					Vector2 sizeDelta = logoContainer.sizeDelta;
					sizeDelta.x = 438f;
					Tween.Add(logoContainer, "sizeDelta", sizeDelta, p_duration, num, Cubic.Out);
					num += p_duration;
					Vector3 localScale = logoSeparator.localScale;
					localScale.x = 1f;
					Tween.Add(logoSeparator, "localScale", localScale, 0.3f, num - p_duration * 0.5f, Cubic.Out);
				}
				else
				{
					Vector3 localScale = logoSeparator.localScale;
					localScale.x = 0f;
					Tween.Add(logoSeparator, "localScale", localScale, 0.3f, num, Cubic.Out);
					num += 0.15f;
					Vector2 sizeDelta = logoContainer.sizeDelta;
					sizeDelta.x = 225f;
					Tween.Add(logoContainer, "sizeDelta", sizeDelta, p_duration, num, Cubic.Out);
				}
			}
		}
	}
}
