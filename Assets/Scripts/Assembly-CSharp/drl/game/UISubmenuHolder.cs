using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	[RequireComponent(typeof(UINavigation))]
	public class UISubmenuHolder : MonoBehaviour
	{
		[Tooltip("Prefab of the submenu to instantiate")]
		[SerializeField]
		private GameObject m_submenuPrefab;

		[Tooltip("Node under where the submenu will be placed")]
		[SerializeField]
		private Transform submenuAnchor;

		[Tooltip("Optional: image component that will show the toggle state")]
		[SerializeField]
		private Image m_submenuToggle;

		[Tooltip("Optional: sprites that indicate the toggle state (Open/Close)")]
		[SerializeField]
		private Sprite[] m_submenuToggleIcons = new Sprite[2];

		private UIChatMessageController mChatMsgController;

		private UIChatSubmenuController mSubMenuController;

		private bool mInitialized;

		private bool mIsPrivate;

		public void Init(string steamId, string userName, string photoURL, bool isFriend, bool isOnline, bool isPrivate, Color color, string p_platform)
		{
			mIsPrivate = isPrivate;
			if (!isPrivate)
			{
				GameObject gameObject = Object.Instantiate(m_submenuPrefab, submenuAnchor);
				mSubMenuController = gameObject.GetComponent<UIChatSubmenuController>();
				UINavigation component = GetComponent<UINavigation>();
				mSubMenuController.Setup(new UIChatSubmenuData(steamId, userName, isFriend, isOnline, isPrivate, photoURL, color, p_platform, component, component.down, m_submenuToggle, m_submenuToggleIcons[0], m_submenuToggleIcons[1]));
				mChatMsgController = base.gameObject.GetComponent<UIChatMessageController>();
				mChatMsgController.onToggleSubmenuPanel = OnToggleSubMenu;
				mSubMenuController.Fold(0f);
				mInitialized = true;
			}
		}

		private void Update()
		{
			if (mInitialized && !mIsPrivate && m_submenuToggle != null)
			{
				m_submenuToggle.gameObject.SetActive((mSubMenuController != null && mSubMenuController.IsOpen) || (UINavigation.focus != null && UINavigation.focus.gameObject == base.gameObject));
			}
		}

		private void OnToggleSubMenu()
		{
			if (mSubMenuController.IsOpen)
			{
				mSubMenuController.Fold();
			}
			else
			{
				mSubMenuController.Unfold();
			}
		}
	}
}
