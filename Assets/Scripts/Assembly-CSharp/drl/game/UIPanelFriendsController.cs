using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPanelFriendsController : Controller<DRLApp>
	{
		public UIPanelFriendsView view => AssertLocal<UIPanelFriendsView>("view");

		public bool submenuOpened { get; set; }

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "service.social.friends@refresh":
				RefreshFriendsList();
				break;
			case "social.friends.order@change":
			{
				DRLStepperView dRLStepperView = p_target as DRLStepperView;
				view.sortingMode = ((dRLStepperView.index != 0) ? Sort.name : Sort.online);
				RefreshFriendsList();
				break;
			}
			case "social.friend.remove-friend-button@click":
				if (p_data.Length != 0)
				{
					string text = (string)p_data[0];
					if (!string.IsNullOrEmpty(text))
					{
						RemoveFriend(text);
					}
				}
				break;
			case "social.friends.search.form@unfocus":
				OnInputFieldEndEdit();
				break;
			}
		}

		protected override void Start()
		{
			base.Start();
			view.searchInput.enabled = true;
			Activity.Run(delegate
			{
				if (base.validContext && SteamManager.Initialized)
				{
					base.app.model.service.social.friends.RefreshFriendsAPI();
				}
			}, 0f, 2f);
		}

		public void RefreshFriendsList()
		{
			List<GameFriendData> list = base.app.model.service.social.friends.list;
			view.Refresh(list);
		}

		public void Populate(List<GameFriendData> p_friends)
		{
			view.Set(p_friends);
		}

		public void RemoveFriend(string p_id)
		{
			base.app.model.service.social.friends.Remove(p_id);
			base.app.model.service.social.friends.RefreshFriendsAPI();
		}

		public void OnInputFieldChanged()
		{
			InputField searchInput = view.searchInput;
			if (string.IsNullOrEmpty(searchInput.text))
			{
				view.ClearSearchResults();
			}
			else
			{
				FilterFriends(searchInput.text);
			}
		}

		public void OnInputFieldEndEdit()
		{
		}

		public void FilterFriends(string p_textQuery)
		{
			List<UIFriendItemView> list = view.listField.GetList<UIFriendItemView>();
			if (list.Count != 0)
			{
				list = list.FindAll((UIFriendItemView f) => f.gameFriendData.name.ToLower().Contains(p_textQuery.ToLower()));
				view.SetSearchResults(list);
			}
		}
	}
}
