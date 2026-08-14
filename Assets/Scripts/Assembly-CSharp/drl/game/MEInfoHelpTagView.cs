using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEInfoHelpTagView : View<DRLApp>
	{
		public List<LayoutElement> spaces;

		public List<Sprite> backgrounds;

		public Image backgroundImage;

		public List<Sprite> icons;

		public Image iconImage;

		public Text labelField;

		public GameObject keyContainer;

		public Text keyField;

		public GameObject separator;

		public MEInfoHelpData data;

		public string label
		{
			get
			{
				return labelField.text;
			}
			set
			{
				labelField.text = value;
				labelField.gameObject.SetActive(!string.IsNullOrEmpty(value));
			}
		}

		public string key
		{
			get
			{
				return keyField.text;
			}
			set
			{
				keyField.text = value;
				keyContainer.SetActive(!string.IsNullOrEmpty(value));
			}
		}

		public bool separatorVisible
		{
			get
			{
				if (!separator)
				{
					return false;
				}
				return separator.activeInHierarchy;
			}
			set
			{
				if ((bool)separator)
				{
					separator.SetActive(value);
				}
			}
		}

		public void Set(string p_label, string p_icon, string p_key, bool p_reverse = false)
		{
			label = p_label;
			SetIcon(p_icon);
			key = p_key;
			separator.transform.SetSiblingIndex(0);
			int siblingIndex = keyField.transform.GetSiblingIndex();
			iconImage.transform.SetSiblingIndex(p_reverse ? siblingIndex : (siblingIndex + 1));
			separator.transform.SetSiblingIndex(3);
		}

		public void Set(MEInfoHelpData p_data)
		{
			data = p_data;
			separatorVisible = false;
			Clear();
			if (data != null)
			{
				separatorVisible = data.separator;
				string text = data.label;
				if (data.localized)
				{
					Localization instance = Localization.instance;
					text = (instance ? instance.Get<string>(data.label, data.defaultLabel) : data.defaultLabel);
				}
				label = text.ToUpper();
				key = data.key;
				separator.transform.SetSiblingIndex(0);
				int siblingIndex = keyContainer.transform.GetSiblingIndex();
				iconImage.sprite = data.icon;
				iconImage.gameObject.SetActive(data.icon != null);
				iconImage.transform.SetSiblingIndex((!data.reversed) ? (siblingIndex - 1) : (siblingIndex + 1));
				separator.transform.SetSiblingIndex(3);
			}
		}

		public void Clear()
		{
			Set("", "", "");
		}

		public void SetIcon(string p_id)
		{
			iconImage.sprite = null;
			for (int i = 0; i < icons.Count; i++)
			{
				if (icons[i].name == p_id)
				{
					iconImage.sprite = icons[i];
					break;
				}
			}
			iconImage.gameObject.SetActive(iconImage.sprite);
		}

		public void SetBackground(int p_id)
		{
			p_id = Mathf.Clamp(p_id, -1, 2);
			backgroundImage.sprite = ((p_id < 0) ? null : backgrounds[p_id]);
			float minWidth = 6f;
			float minWidth2 = 6f;
			switch (p_id)
			{
			case -1:
				minWidth = 5f;
				minWidth2 = 2f;
				break;
			case 0:
				minWidth = 2f;
				minWidth2 = 6f;
				break;
			case 1:
				minWidth = 8f;
				minWidth2 = 6f;
				break;
			case 2:
				minWidth = 8f;
				minWidth2 = 0f;
				break;
			}
			spaces[0].minWidth = minWidth;
			spaces[1].minWidth = minWidth2;
		}
	}
}
