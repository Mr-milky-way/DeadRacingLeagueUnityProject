using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class LocaleText : LocaleElement
	{
		public enum Case
		{
			None = 0,
			Upper = 1,
			Lower = 2
		}

		[SerializeField]
		private Text m_target;

		public Case textCase;

		public Text target
		{
			get
			{
				return m_target ?? (m_target = GetComponent<Text>());
			}
			set
			{
				m_target = value;
			}
		}

		public override void OnLocaleRefresh()
		{
			if (this == null || base.gameObject == null || !target || keys.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < keys.Count; i++)
			{
				string text = keys[i];
				if (text.Contains("@font-size"))
				{
					int fontSize = base.manager.Get(text, target.fontSize);
					target.fontSize = fontSize;
					continue;
				}
				string text2 = base.manager.Get<string>(text, target.text);
				switch (textCase)
				{
				case Case.Upper:
					text2 = text2.ToUpper();
					break;
				case Case.Lower:
					text2 = text2.ToLower();
					break;
				}
				target.text = text2;
			}
		}
	}
}
