using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UICardButtonGarageEditItem : UICardView
	{
		public Text title0Field;

		public Text title1Field;

		public RawImage previewField;

		public RawImage imageField;

		public GameObject unallowedField;

		public GameObject promoField;

		public GameObject wipField;

		public Text normalTitle0Field;

		public FadeComponent normalContentFade;

		public FadeComponent contentFade;

		public Image outlineField;

		public GameObject content;

		public GameObject focus;

		public UINavigation navigation;

		private bool m_selected;

		private bool m_focus;

		public new DRLAsset data;

		public DRLStoreAsset storeData;

		public bool unallowed;

		public override UICardType type => UICardType.ButtonGarageEditItem;

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

		public void Set(DRLAsset p_data)
		{
			string[] array = ((p_data == null) ? new string[0] : p_data.info.name.Split('\n'));
			string text = string.Empty;
			storeData = ((p_data == null) ? null : p_data.GetComponent<DRLStoreAsset>());
			if (storeData != null && storeData.inDevelopment)
			{
				normalContentFade.FadeIn(0.1f);
				if (array.Length == 0)
				{
					array = new string[1] { "" };
				}
				else
				{
					normalTitle0Field.text = array[0].ToUpper();
				}
				imageField.gameObject.SetActive(value: false);
				unallowedField.SetActive(value: false);
				wipField.SetActive(value: true);
			}
			else
			{
				imageField.gameObject.SetActive(value: true);
				if (unallowed)
				{
					unallowedField.SetActive(value: true);
				}
				else
				{
					unallowedField.SetActive(value: false);
				}
				wipField.SetActive(value: false);
				if (normalContentFade.alpha > 0f)
				{
					normalContentFade.FadeOut(0.1f);
				}
				if (p_data is DroneBattery)
				{
					text = ((DroneBattery)p_data).capacity + " mAh";
				}
				else if (!(p_data is DroneFrame))
				{
					if (p_data is DroneMotor)
					{
						text = ((DroneMotor)p_data).spec.kv + " kV";
					}
					else if (p_data is DroneProp)
					{
						text = ((DroneProp)p_data).diameter + "\" x " + ((DroneProp)p_data).pitch + "\"";
					}
					else
					{
						_ = p_data is DroneSkin;
					}
				}
			}
			promoField.SetActive(storeData != null && storeData.isPromo);
			if ((bool)title0Field)
			{
				title0Field.gameObject.SetActive(array.Length != 0);
			}
			if ((bool)title1Field)
			{
				title1Field.gameObject.SetActive(array.Length > 1);
			}
			title0 = ((array.Length != 0) ? array[0] : "").ToUpper();
			title1 = ((array.Length > 1) ? array[1] : text);
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
			contentFade.alpha = -0.1f;
			base.interactable = p_data;
			if (!p_data)
			{
				outlineField.color = DRLColor.gray3;
				content.SetActive(value: false);
				focus.SetActive(value: false);
			}
			else
			{
				content.SetActive(value: true);
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
			if (storeData != null && storeData.inDevelopment)
			{
				normalContentFade.Fade(0f, 0.2f, 0f, Cubic.Out);
				wipField.GetComponent<FadeComponent>().FadeOut(0.2f);
			}
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			m_focus = false;
			contentFade.Fade(m_selected ? 1f : (-0.1f), 0.2f, 0f, Cubic.Out);
			if (storeData != null && storeData.inDevelopment)
			{
				normalContentFade.Fade(1f, 0.2f, 0f, Cubic.Out);
				wipField.GetComponent<FadeComponent>().FadeIn(0.2f);
			}
		}
	}
}
