using UnityEngine;

namespace drl.game
{
	public class UIMapsCategoryView : UIScreenView
	{
		public int depth;

		public RectTransform virtualSeasonContainer;

		public RectTransform communityMapsContainer;

		[SerializeField]
		public GameObject collectableCards;

		[Header("Favorite Maps")]
		[SerializeField]
		private UIStatusView favoriteUIStatusView;

		[SerializeField]
		private Transform favoriteDisabledImage;

		[SerializeField]
		private UICardButtonMap favoriteUICardButtonMap;

		[Header("Community Tracks")]
		[SerializeField]
		private UIStatusView communityMapsUIStatusView;

		[SerializeField]
		private Transform communityMapsDisabledImage;

		[SerializeField]
		private UICardButtonMap communityMapsUICardButtonMap;

		private void Awake()
		{
			favoriteUIStatusView.message = base.app.model.storage.locale.Get("maps.favorite-maps.no-maps-selected", "NO TRACKS MARKED AS FAVORITE");
			favoriteUIStatusView.icon = "warning";
		}

		public void SetFavoriteMapsCardsEnabled(bool p_enabled)
		{
			favoriteDisabledImage.gameObject.SetActive(!p_enabled);
			favoriteUICardButtonMap.enabled = p_enabled;
			favoriteUIStatusView.fade.alpha = (p_enabled ? 0f : 1f);
		}

		public void SetCommunityMapsCardsEnabled(bool p_enabled)
		{
			communityMapsDisabledImage.gameObject.SetActive(!p_enabled);
			communityMapsUICardButtonMap.enabled = p_enabled;
			communityMapsUIStatusView.fade.alpha = (p_enabled ? 0f : 1f);
			if (!p_enabled)
			{
				string warning = base.app.model.storage.locale.Get("ui.offline.status", "UNAVAILABLE (OFFLINE)");
				communityMapsUIStatusView.SetWarning(warning);
				communityMapsUIStatusView.fade.alpha = 1f;
			}
		}
	}
}
