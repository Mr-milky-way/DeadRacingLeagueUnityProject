using UnityEngine;

namespace drl.game
{
	public class UIMapsSDCategoryView : UIScreenView
	{
		public int depth;

		[Header("Favorite Maps")]
		[SerializeField]
		private UIStatusView favoriteUIStatusView;

		[SerializeField]
		private Transform favoriteDisabledImage;

		[SerializeField]
		private UICardButtonMap favoriteUICardButtonMap;

		private void Awake()
		{
			favoriteUIStatusView.message = base.app.model.storage.locale.Get("maps.favorite-maps.no-maps-selected", "EMPTY. ADD TRACKS USING EACH TRACK'S HEART ICON.");
			favoriteUIStatusView.icon = "warning";
		}

		public void SetFavoriteMapsCardsEnabled(bool p_enabled)
		{
			favoriteDisabledImage.gameObject.SetActive(!p_enabled);
			favoriteUICardButtonMap.enabled = p_enabled;
			favoriteUIStatusView.fade.alpha = (p_enabled ? 0f : 0.5f);
		}
	}
}
