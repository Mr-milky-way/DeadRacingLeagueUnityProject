using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIStatusView : View<DRLApp>
	{
		public List<RectTransform> icons;

		public Text messageField;

		public LayoutElement messageFieldLayout;

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public string message
		{
			get
			{
				return messageField.text;
			}
			set
			{
				messageField.text = value;
			}
		}

		public string icon
		{
			get
			{
				for (int i = 0; i < icons.Count; i++)
				{
					if (icons[i].gameObject.activeInHierarchy)
					{
						return icons[i].name;
					}
				}
				return "";
			}
			set
			{
				for (int i = 0; i < icons.Count; i++)
				{
					icons[i].gameObject.SetActive(icons[i].name == value);
				}
			}
		}

		public void SetLoading(float p_progress)
		{
			if (icon != "prop")
			{
				icon = "prop";
			}
			messageFieldLayout.minWidth = 0f;
			message = base.app.model.storage.locale.Get("ui.common.loading-w-dots", "LOADING...");
			if (!(p_progress <= 0f))
			{
				message = base.app.model.storage.locale.Get("ui.label.loading@upper", "LOADING") + "<color=red>/</color> " + Mathf.FloorToInt(Mathf.Clamp01(p_progress) * 100f) + "%";
			}
		}

		public void SetWarning(string p_message)
		{
			if (icon != "warning")
			{
				icon = "warning";
			}
			messageFieldLayout.minWidth = 0f;
			message = p_message;
		}
	}
}
