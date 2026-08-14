using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	[RequireComponent(typeof(Element))]
	public class DRLTooltipTrigger : LocaleElement, IFocusHandler
	{
		[HideInInspector]
		[SerializeField]
		private Element m_element;

		public string text;

		public LocaleText.Case textCase;

		public Element element
		{
			get
			{
				if (m_element == null)
				{
					m_element = GetComponent<Element>();
					if (m_element == null)
					{
						Debug.LogError("DRLTooltipTrigger> No UIElement attached on trigger " + base.name);
						base.enabled = false;
					}
				}
				return m_element;
			}
		}

		public override void OnLocaleRefresh()
		{
			if (this == null || base.gameObject == null || !element || keys.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < keys.Count; i++)
			{
				string p_key = keys[i];
				text = base.manager.Get<string>(p_key, text);
				switch (textCase)
				{
				case LocaleText.Case.Upper:
					text = text.ToUpper();
					break;
				case LocaleText.Case.Lower:
					text = text.ToLower();
					break;
				}
			}
		}

		public void OnFocus()
		{
			if (base.enabled && (bool)element)
			{
				element.Notify("ui.tooltip@show", text);
			}
		}

		public void OnUnfocus()
		{
			if (base.enabled && (bool)element)
			{
				element.Notify("ui.tooltip@hide");
			}
		}
	}
}
