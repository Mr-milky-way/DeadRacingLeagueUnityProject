using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UIStoreProductItemView : UICardView
	{
		public RawImage productPhotoField;

		public Image productIcon;

		public Sprite[] categoryIcons;

		public RectTransform assetPhotoContainer;

		public List<RectTransform> assetPhotoRows;

		public List<RawImage> assetPhotoRow0;

		public List<RawImage> assetPhotoRow1;

		[Header("Navigation")]
		public UINavigation buttonPreviewNav;

		public UINavigation buttonBuyNav;

		public new DRLStoreProductData data;

		public List<DRLAsset> productParts;

		private bool m_selected;

		[SerializeField]
		private FadeComponent m_productFade;

		private AsyncRequest m_photo_loader;

		private AsyncRequest m_thumnbnail_loader;

		private AsyncRequest m_leader_loader;

		private Texture2D m_thumb_file_texture;

		public Color limitedColor;

		public Color unlimitedColor;

		public Text limitedText;

		public Image limitedBackground;

		public Image featuredIcon;

		public Text productNameText;

		public Text priceText;

		public override bool selected
		{
			get
			{
				return m_selected;
			}
			set
			{
				bool flag = value;
				m_selected = flag;
			}
		}

		public Texture productPhoto
		{
			set
			{
				UIReflection.Set(productPhotoField, value);
				if ((bool)productPhotoField)
				{
					productPhotoField.enabled = value;
				}
			}
		}

		public FadeComponent productImageFade
		{
			get
			{
				if (!m_productFade)
				{
					return m_productFade = (productPhotoField ? productPhotoField.GetComponent<FadeComponent>() : null);
				}
				return m_productFade;
			}
		}

		public void Set(DRLStoreProductData p_data)
		{
			if (!base.validContext)
			{
				return;
			}
			data = p_data;
			if (data != null)
			{
				if (m_photo_loader != null)
				{
					m_photo_loader.Cancel();
				}
				if (m_thumnbnail_loader != null)
				{
					m_thumnbnail_loader.Cancel();
				}
				if (m_leader_loader != null)
				{
					m_leader_loader.Cancel();
				}
				productImageFade.alpha = 0f;
				List<string> p_guids = new List<string>(data.items);
				List<DRLAsset> list = new List<DRLAsset>();
				list.AddRange(base.app.model.storage.library.FindByGUID<DronePart>(p_guids));
				productParts = list;
				Debug.Log(string.Format("UIStoreProductItemView> Set / count[{0}]\n{1}", list.Count, string.Join("\n", data.items)));
				List<Texture2D> assetPhotos = list.ConvertAll((DRLAsset it) => it.info.thumb);
				SetAssetPhotos(assetPhotos);
				productImageFade.FadeIn();
				productNameText.text = data.name.ToUpper();
				priceText.text = base.app.model.service.platform.GetProductPriceString(data.platformId);
				_ = p_data.category;
				if (data.limited)
				{
					limitedText.text = $"LIMITED {data.currentAvailableAmount}/{data.maxAvailableAmount}";
					limitedBackground.color = limitedColor;
				}
				else
				{
					limitedText.text = string.Empty;
					limitedBackground.color = unlimitedColor;
				}
				if (data.featured)
				{
					featuredIcon.enabled = true;
				}
				else
				{
					featuredIcon.enabled = false;
				}
			}
		}

		[ContextMenu("Set Asset Photos Debug")]
		public void SetAssetPhotosDebug()
		{
		}

		public void SetAssetPhotos(List<Texture2D> p_list)
		{
			int count = p_list.Count;
			int num = Mathf.Min(count / 4 + 1, 2);
			int num2 = Mathf.CeilToInt((float)count / (float)num);
			for (int i = 0; i < assetPhotoRows.Count; i++)
			{
				assetPhotoRows[i].gameObject.SetActive(i <= num - 1);
			}
			for (int j = 0; j < assetPhotoRow0.Count; j++)
			{
				assetPhotoRow0[j].gameObject.SetActive(value: false);
				assetPhotoRow0[j].GetComponent<ImageLayout>().align = ((num <= 1) ? ImageLayout.Aligment.Center : ImageLayout.Aligment.BottomCenter);
			}
			for (int k = 0; k < assetPhotoRow1.Count; k++)
			{
				assetPhotoRow1[k].gameObject.SetActive(value: false);
			}
			int num3 = 0;
			TextureWrapMode wrapMode = TextureWrapMode.Clamp;
			for (int l = 0; l < num2; l++)
			{
				if (num3 >= p_list.Count)
				{
					break;
				}
				if (l >= assetPhotoRow0.Count)
				{
					break;
				}
				Texture2D texture = p_list[num3];
				num3++;
				assetPhotoRow0[l].gameObject.SetActive(value: true);
				assetPhotoRow0[l].texture = texture;
				assetPhotoRow0[l].texture.wrapMode = wrapMode;
				assetPhotoRow0[l].GetComponent<LayoutElement>().preferredWidth = ((num >= 2) ? 75 : 150);
			}
			for (int m = 0; m < num2; m++)
			{
				if (num3 >= p_list.Count)
				{
					break;
				}
				if (m >= assetPhotoRow1.Count)
				{
					break;
				}
				Texture2D texture2 = p_list[num3];
				num3++;
				assetPhotoRow1[m].gameObject.SetActive(value: true);
				assetPhotoRow1[m].texture = texture2;
				assetPhotoRow1[m].texture.wrapMode = wrapMode;
				assetPhotoRow1[m].GetComponent<LayoutElement>().preferredWidth = ((num >= 2) ? 75 : 150);
			}
		}

		public void SetPhotoLayout(bool p_is_image)
		{
			productPhotoField.gameObject.SetActive(p_is_image);
			assetPhotoContainer.gameObject.SetActive(!p_is_image);
		}

		public void LinkRight(UIStoreProductItemView p_item, UINavigation backButton = null, bool lastCard = false)
		{
			buttonPreviewNav.right = buttonBuyNav;
			if ((bool)backButton)
			{
				buttonPreviewNav.left = backButton;
			}
			buttonBuyNav.left = buttonPreviewNav;
			if (!lastCard)
			{
				buttonBuyNav.right = p_item.buttonPreviewNav;
				p_item.buttonPreviewNav.left = buttonBuyNav;
			}
			else
			{
				buttonBuyNav.right = null;
			}
		}

		public void LinkDown(UIStoreProductItemView p_item, UINavigation stepperNav = null)
		{
			if (stepperNav != null)
			{
				buttonBuyNav.down = p_item.buttonBuyNav;
				buttonPreviewNav.down = p_item.buttonPreviewNav;
				buttonBuyNav.up = stepperNav;
				buttonPreviewNav.up = stepperNav;
			}
			else
			{
				buttonBuyNav.up = p_item.buttonBuyNav;
				buttonPreviewNav.up = p_item.buttonPreviewNav;
				p_item.buttonBuyNav.up = buttonBuyNav;
				p_item.buttonPreviewNav.up = buttonPreviewNav;
			}
		}
	}
}
