using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UIHomeView : UIScreenView
	{
		public UICardButtonLarge vdrl;

		public LayoutGroup leftMostLayout;

		public LayoutGroup rowTopLayout;

		public LayoutGroup rowBtmLayout;

		public ListComponent leftList;

		public ListComponent rowTopList;

		public ListComponent rowBtmList;

		public Text socialOnlinePlayerField;

		public List<UICardButtonLarge> multiplayerContextCards;

		public List<UICardButtonLarge> onlineContextCards;

		public List<UICardButtonLarge> crossplayContextCards;

		public DRLMap usafMapData;

		public void SetMultiplayerContextEnabled(bool p_flag, bool p_interactable, string p_caption)
		{
			for (int i = 0; i < multiplayerContextCards.Count; i++)
			{
				UICardButtonLarge uICardButtonLarge = multiplayerContextCards[i];
				uICardButtonLarge.enabled = p_interactable;
				bool flag = uICardButtonLarge.notification.Contains("multiplayer");
				Component component = uICardButtonLarge.Find<Component>("backgrounds.disabled");
				VerticalLayoutGroup verticalLayoutGroup = uICardButtonLarge.Find<VerticalLayoutGroup>("content.body");
				UIStatusView uIStatusView = uICardButtonLarge.Find<UIStatusView>("content.status");
				if (component != null)
				{
					component.gameObject.SetActive(!p_flag);
					uIStatusView.SetWarning(p_caption);
					uIStatusView.fade.alpha = (p_flag ? 0f : 1f);
					RectOffset padding = verticalLayoutGroup.padding;
					verticalLayoutGroup.enabled = false;
					padding.top = (flag ? ((!p_flag) ? (-90) : 0) : 0);
					verticalLayoutGroup.padding = padding;
					verticalLayoutGroup.enabled = true;
				}
			}
		}

		public void RefreshOnlineContextCards()
		{
			bool flag = !DRLApp.offline;
			string warning = (flag ? "" : base.app.model.storage.locale.Get("ui.offline.status", "UNAVAILABLE (OFFLINE)"));
			for (int i = 0; i < onlineContextCards.Count; i++)
			{
				UICardButtonLarge uICardButtonLarge = onlineContextCards[i];
				uICardButtonLarge.enabled = flag;
				bool flag2 = uICardButtonLarge.notification.Contains("multiplayer");
				Component component = uICardButtonLarge.Find<Component>("backgrounds.disabled");
				VerticalLayoutGroup verticalLayoutGroup = uICardButtonLarge.Find<VerticalLayoutGroup>("content.body");
				UIStatusView uIStatusView = uICardButtonLarge.Find<UIStatusView>("content.status");
				if (component != null)
				{
					component.gameObject.SetActive(!flag);
					uIStatusView.SetWarning(warning);
					uIStatusView.fade.alpha = (flag ? 0f : 1f);
					RectOffset padding = verticalLayoutGroup.padding;
					verticalLayoutGroup.enabled = false;
					padding.top = (flag2 ? ((!flag) ? (-90) : 0) : 0);
					verticalLayoutGroup.padding = padding;
					verticalLayoutGroup.enabled = true;
				}
			}
		}

		public void SetCrossplayContextEnabled(bool p_flag, string p_caption)
		{
			if (crossplayContextCards == null)
			{
				return;
			}
			for (int i = 0; i < crossplayContextCards.Count; i++)
			{
				UICardButtonLarge uICardButtonLarge = crossplayContextCards[i];
				uICardButtonLarge.enabled = p_flag;
				bool flag = uICardButtonLarge.notification.Contains("multiplayer");
				Component component = uICardButtonLarge.Find<Component>("backgrounds.disabled");
				VerticalLayoutGroup verticalLayoutGroup = uICardButtonLarge.Find<VerticalLayoutGroup>("content.body");
				UIStatusView uIStatusView = uICardButtonLarge.Find<UIStatusView>("content.status");
				uICardButtonLarge.interactable = p_flag;
				if (component != null)
				{
					component.gameObject.SetActive(!p_flag);
					uIStatusView.SetWarning(p_caption.ToUpper());
					uIStatusView.fade.alpha = (p_flag ? 0f : 1f);
					RectOffset padding = verticalLayoutGroup.padding;
					verticalLayoutGroup.enabled = false;
					padding.top = (flag ? ((!p_flag) ? (-90) : 0) : 0);
					verticalLayoutGroup.padding = padding;
					verticalLayoutGroup.enabled = true;
				}
			}
		}

		public void SetUserOnlineField(int p_count)
		{
			if ((bool)socialOnlinePlayerField)
			{
				FadeComponent fadeComponent = Hierarchy.FindReverse<FadeComponent>(socialOnlinePlayerField.transform);
				if ((bool)fadeComponent)
				{
					fadeComponent.Fade((p_count <= 0) ? 0f : 1f, 0.2f);
				}
				socialOnlinePlayerField.text = p_count.ToString() ?? "";
			}
		}

		public void SetUserOnlineFieldAlpha(float p_value)
		{
			if ((bool)socialOnlinePlayerField)
			{
				FadeComponent fadeComponent = Hierarchy.FindReverse<FadeComponent>(socialOnlinePlayerField.transform);
				if ((bool)fadeComponent)
				{
					fadeComponent.alpha = p_value;
				}
			}
		}

		public void RefreshPromoBanner()
		{
			RawImage promoContentImage = base.app.view.ui.screens.promoContentImage;
			if (promoContentImage == null)
			{
				return;
			}
			if (!base.current)
			{
				base.app.view.ui.screens.promoContentImage.texture = null;
				base.app.view.ui.screens.promoContentImage.gameObject.SetActive(value: false);
				return;
			}
			Web.Get("drl.service.home.promo-banner", DRLService.promoBannerUri, delegate(Texture2D data, float progress, WebAsyncRequest request)
			{
				if (request.progress >= 1f && !(promoContentImage == null))
				{
					if (request.code != 200 || request.hasError || request.status == AsyncRequestStatus.Error)
					{
						promoContentImage.texture = null;
						promoContentImage.gameObject.SetActive(value: false);
					}
					else if (data == null || !base.current)
					{
						promoContentImage.texture = null;
						promoContentImage.gameObject.SetActive(value: false);
					}
					else
					{
						Debug.Log("RefreshPromoBanner> Promo banner set.");
						data.wrapMode = TextureWrapMode.Clamp;
						data.minimumMipmapLevel = 0;
						promoContentImage.texture = data;
						promoContentImage.gameObject.SetActive(value: true);
					}
				}
			});
		}
	}
}
