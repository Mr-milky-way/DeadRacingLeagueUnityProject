using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class MEControlsWidget : NotificationView<DRLApp>
	{
		public List<Button> menu;

		public bool follow = true;

		protected virtual void Awake()
		{
			for (int i = 0; i < menu.Count; i++)
			{
				menu[i].onClick.AddListener(GetButtonListener(menu[i]));
			}
		}

		private UnityAction GetButtonListener(Button b)
		{
			return delegate
			{
				OnMenuClick(b);
			};
		}

		protected void OnMenuClick(Button p_button)
		{
			Notify(notification + "@click", p_button);
		}
	}
}
