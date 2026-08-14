using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class UISpectateCTButton : UIElementView<DRLApp>
	{
		public RectTransform contentContainer;

		public Image focusField;

		public Text numberField;

		public Image outlineActiveField;

		public List<Image> hintFieldList;

		public RectTransform hintContainer;

		public CanvasGroup group;

		public int index;

		public bool IsEnabled()
		{
			return group.alpha >= 1f;
		}

		public void SetLabel(string p_value)
		{
			numberField.text = p_value;
		}

		public void SetEnabled(bool p_flag)
		{
			group.alpha = (p_flag ? 1f : 0.2f);
			CanvasGroup canvasGroup = group;
			bool blocksRaycasts = (group.interactable = p_flag);
			canvasGroup.blocksRaycasts = blocksRaycasts;
		}

		public void SetActive(bool p_flag)
		{
			outlineActiveField.enabled = p_flag;
		}

		public void SetFocus(bool p_flag)
		{
			focusField.enabled = p_flag;
		}

		public void SetHintList(bool p_flag)
		{
			hintContainer.gameObject.SetActive(p_flag);
			Vector2 sizeDelta = contentContainer.sizeDelta;
			sizeDelta.x = (p_flag ? 27f : 32f);
			contentContainer.sizeDelta = sizeDelta;
		}

		public void SetHint(bool p_flag, int p_index, Color p_color)
		{
			if (p_index >= 0 && p_index < hintFieldList.Count)
			{
				hintFieldList[p_index].enabled = p_flag;
				hintFieldList[p_index].color = p_color;
			}
		}

		public void SetHint(bool p_flag, int p_index)
		{
			SetHint(p_flag, p_index, DRLColor.gray3);
		}

		public void SetHint(bool p_flag)
		{
			for (int i = 0; i < hintFieldList.Count; i++)
			{
				SetHint(p_flag, i);
			}
		}
	}
}
