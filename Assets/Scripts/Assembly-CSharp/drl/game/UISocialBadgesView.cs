using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UISocialBadgesView : View<DRLApp>
	{
		public UISocialView socialView;

		public CanvasGroup socialButtonGlobalNotificationCanvasGroup => base.app.view.ui.footer.socialButtonGlobalNotificationCanvasGroup;

		public CanvasGroup socialButtonPrivateNotificationCanvasGroup => base.app.view.ui.footer.socialButtonPrivateNotificationCanvasGroup;

		public bool initialized { get; set; }

		public void SetSocialButtonGlobalDirty()
		{
			socialButtonGlobalNotificationCanvasGroup.gameObject.SetActive(value: true);
			socialButtonGlobalNotificationCanvasGroup.alpha = 1f;
		}

		public void SetSocialButtonGlobalClear()
		{
			socialButtonGlobalNotificationCanvasGroup.gameObject.SetActive(value: false);
			socialButtonGlobalNotificationCanvasGroup.alpha = 0f;
		}
	}
}
