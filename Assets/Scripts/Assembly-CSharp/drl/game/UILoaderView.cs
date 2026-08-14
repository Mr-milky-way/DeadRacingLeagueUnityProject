using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UILoaderView : View<DRLApp>
	{
		public FadeComponent fade;

		public RawImage backgroundField;

		public Image barField;

		public Texture defaultBackground;

		public Text captionField;

		public Text descriptionField;

		public GameObject footerContainer;

		public List<GameObject> infoList;

		public GameObject hotkeysPC;

		public GameObject hotkeysXbox;

		public GameObject hotkeysPS;

		public Transform hotkeysPSL1;

		public Transform hotkeysPSR1;

		public Image forfeitButtonIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		[SerializeField]
		private CanvasGroup forfeitPSBtn;

		public float progress
		{
			get
			{
				if ((bool)barField)
				{
					return barField.transform.localScale.x;
				}
				return 0f;
			}
			set
			{
				Vector3 localScale = (barField ? barField.transform.localScale : Vector3.zero);
				localScale.x = Mathf.Clamp01(value);
				if ((bool)barField)
				{
					barField.transform.localScale = localScale;
				}
			}
		}

		public Texture background
		{
			get
			{
				if (!backgroundField)
				{
					return null;
				}
				return backgroundField.texture;
			}
			set
			{
				if ((bool)backgroundField)
				{
					backgroundField.texture = value;
					backgroundField.color = new Color(1f, 1f, 1f, value ? 1f : 0f);
				}
			}
		}

		public Color tint
		{
			get
			{
				if (!backgroundField)
				{
					return Color.clear;
				}
				return backgroundField.color;
			}
			set
			{
				if ((bool)backgroundField)
				{
					backgroundField.color = value;
				}
			}
		}

		public void SetFooter(string p_caption, string p_description, LoaderFooterInfo p_info_flag)
		{
			if ((bool)captionField)
			{
				captionField.gameObject.SetActive(!string.IsNullOrEmpty(p_caption));
				captionField.text = p_caption;
			}
			if ((bool)descriptionField)
			{
				descriptionField.gameObject.SetActive(!string.IsNullOrEmpty(p_description));
				descriptionField.text = p_description;
			}
			bool flag = string.IsNullOrEmpty(p_caption) && string.IsNullOrEmpty(p_description);
			if ((bool)footerContainer)
			{
				footerContainer.SetActive(!flag);
			}
			GameObject gameObject = infoList[0];
			if ((bool)gameObject)
			{
				gameObject.SetActive((p_info_flag & LoaderFooterInfo.Hotkeys) != 0);
			}
			gameObject = infoList[1];
			if ((bool)gameObject)
			{
				gameObject.SetActive((p_info_flag & LoaderFooterInfo.Promo) != 0);
			}
			gameObject = infoList[2];
			if ((bool)gameObject)
			{
				gameObject.SetActive((p_info_flag & LoaderFooterInfo.Workbench) != 0);
			}
			RefreshNavigationTooltips();
		}

		public void SetFooter(string p_caption, string p_description)
		{
			SetFooter(p_caption, p_description, LoaderFooterInfo.None);
		}

		public void ClearFooter()
		{
			SetFooter("", "");
		}

		public void OnPersistency()
		{
			if ((bool)base.app.view.ui)
			{
				base.app.view.ui.loader = this;
			}
		}

		public void RefreshNavigationTooltips()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			bool flag = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
			bool flag2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
			if ((bool)hotkeysXbox)
			{
				hotkeysXbox.SetActive(flag);
			}
			if ((bool)hotkeysPS)
			{
				hotkeysPS.SetActive(flag2);
				hotkeysPSL1.localScale = Vector3.one * 1.3f;
				hotkeysPSR1.localScale = Vector3.one * 1.3f;
			}
			if ((bool)hotkeysPC)
			{
				hotkeysPC.SetActive(!flag && !flag2);
			}
		}
	}
}
