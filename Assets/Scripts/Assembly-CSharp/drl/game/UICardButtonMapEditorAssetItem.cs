using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICardButtonMapEditorAssetItem : UICardView
	{
		public Text title0Field;

		public Text title1Field;

		public RawImage previewField;

		public RawImage imageField;

		public FadeComponent contentFade;

		public FadeComponent fade;

		public FadeComponent containerFade;

		public Image outlineField;

		public GameObject focus;

		public UINavigation navigation;

		private bool m_selected;

		private bool m_focus;

		public new MapAsset data;

		public override UICardType type => UICardType.ButtonMapEditorAssetItem;

		public string title0
		{
			set
			{
				UIReflection.Set(title0Field, value);
			}
		}

		public string title1
		{
			set
			{
				UIReflection.Set(title1Field, value);
			}
		}

		public Texture preview
		{
			set
			{
				UIReflection.Set(previewField, value);
				if ((bool)previewField)
				{
					previewField.enabled = value;
				}
			}
		}

		public Texture image
		{
			set
			{
				UIReflection.Set(imageField, value);
				if ((bool)imageField)
				{
					imageField.enabled = value;
				}
			}
		}

		public override bool selected
		{
			get
			{
				return m_selected;
			}
			set
			{
				bool flag = (m_selected = value);
				uint v = 8371755u;
				Tween.Kill(outlineField);
				if ((bool)title0Field)
				{
					Tween.Kill(title0Field);
				}
				if ((bool)title1Field)
				{
					Tween.Kill(title1Field);
				}
				Tween.Add(outlineField, "color", flag ? Colorf.RGBToColor(v) : Colorf.transparent, 0.15f, Cubic.Out);
				if ((bool)title0Field)
				{
					Tween.Add(title0Field, "color", flag ? Colorf.RGBToColor(v) : Color.white, 0.15f, Cubic.Out);
				}
				if ((bool)title1Field)
				{
					Tween.Add(title1Field, "color", flag ? Colorf.RGBToColor(v) : Color.white, 0.15f, Cubic.Out);
				}
				contentFade.Fade(m_focus ? 1f : (m_selected ? 1f : (-0.1f)), 0.2f, 0f, Cubic.Out);
			}
		}

		public void Set(MapAsset p_data)
		{
			string[] array = ((p_data == null) ? new string[0] : p_data.info.name.Split('\n'));
			string empty = string.Empty;
			imageField.gameObject.SetActive(value: true);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Replace(' ', '\n');
			}
			if ((bool)title0Field)
			{
				title0Field.gameObject.SetActive(array.Length != 0);
			}
			if ((bool)title1Field)
			{
				title1Field.gameObject.SetActive(array.Length > 1);
			}
			title0 = ((array.Length != 0) ? array[0] : "").ToUpper();
			title1 = ((array.Length > 1) ? array[1] : empty);
			preview = null;
			image = ((p_data == null) ? null : p_data.info.thumb);
			data = p_data;
			m_selected = false;
			outlineField.color = Colorf.transparent;
			if ((bool)title0Field)
			{
				title0Field.color = Color.white;
			}
			if ((bool)title1Field)
			{
				title1Field.color = Color.white;
			}
			containerFade.allowMouseInput = p_data != null;
			containerFade.alpha = ((p_data == null) ? 0.25f : 1f);
			if (!p_data)
			{
				outlineField.color = DRLColor.gray3;
				focus.SetActive(value: false);
			}
			else
			{
				focus.SetActive(value: true);
			}
		}

		public override void Build()
		{
			base.Build();
		}

		public override void OnFocus()
		{
			base.OnFocus();
			m_focus = true;
			contentFade.Fade(m_selected ? 1f : 1f, 0.2f, 0f, Cubic.Out);
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			m_focus = false;
			contentFade.Fade(m_selected ? 1f : (-0.1f), 0.2f, 0f, Cubic.Out);
		}
	}
}
