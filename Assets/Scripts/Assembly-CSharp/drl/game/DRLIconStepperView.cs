using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class DRLIconStepperView : DRLStepperView
	{
		public Image iconField;

		public List<Sprite> icons;

		protected override void Awake()
		{
			base.Awake();
			if (labels.Length != icons.Count)
			{
				iconField.gameObject.SetActive(value: false);
			}
			else
			{
				iconField.sprite = icons[index];
			}
		}

		protected override void OnChange()
		{
			base.OnChange();
			if (iconField.isActiveAndEnabled && icons != null && icons.Count > index)
			{
				iconField.sprite = icons[index];
			}
		}
	}
}
