using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapSDOverviewController : Controller<DRLApp>
	{
		private bool m_lockUI;

		public UIMapSDOverviewView view => AssertLocal<UIMapSDOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				UIScreen uIScreen = p_data[0] as UIScreen;
				if (!(uIScreen != view.screen))
				{
					LoadCards(uIScreen);
					base.app.arguments.game.type = GameFlag.Collectable;
					UINavigationScroll component = GetComponent<UINavigationScroll>();
					if ((bool)component)
					{
						component.forceScrollX = true;
					}
				}
				break;
			}
			case "fly.map-track-card@click":
			{
				if (m_lockUI)
				{
					break;
				}
				GameFlag type = base.app.arguments.game.type;
				if (type == GameFlag.Collectable && type != GameFlag.MapEditor)
				{
					UICardButtonMapTrack uICardButtonMapTrack = p_target as UICardButtonMapTrack;
					if ((bool)uICardButtonMapTrack)
					{
						string text = (uICardButtonMapTrack.data ? uICardButtonMapTrack.data.map.guid : uICardButtonMapTrack.customData.mapId);
						string text2 = (uICardButtonMapTrack.data ? uICardButtonMapTrack.data.guid : "");
						string text3 = ((uICardButtonMapTrack.customData == null) ? "" : uICardButtonMapTrack.customData.guid);
						bool flag = !string.IsNullOrEmpty(text3);
						DRLMap dRLMap = (uICardButtonMapTrack.data ? uICardButtonMapTrack.data.map : base.app.model.storage.library.FindByGUID<DRLMap>(text));
						DRLMapTrack data = uICardButtonMapTrack.data;
						MapData customData = uICardButtonMapTrack.customData;
						Notify("maps.track-selection-complete", text, text2, text3, flag, dRLMap, data, customData);
					}
				}
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}

		private void LoadCards(UIScreen p_screen)
		{
			List<MapData> list = base.app.model.storage.maps.FetchSDMaps();
			if (list != null && list.Count > 0)
			{
				Debug.Log("UIMapSDOverviewController> LoadCards: Found SD maps locally [" + list.Count + "]");
				view.Set(view.data, list);
			}
			else
			{
				base.app.model.service.GetSDMaps(p_has_root: false, delegate(DRLCommunityMapResult p_result)
				{
					if (base.validContext)
					{
						List<MapData> content_list = new List<MapData>();
						Debug.Log("UIMapSDOverviewController> DRLCommunityMapData: " + p_result.data.Length);
						new Thread((ThreadStart)delegate
						{
							for (int i = 0; i < p_result.data.Length; i++)
							{
								MapData mapData = p_result.data[i].Convert<MapData>();
								content_list.Add(mapData);
								if (mapData == null)
								{
									Debug.LogWarning("UIMultiplayerRoomController> LoadCards / Failed to Parse MapData");
								}
							}
							Activity.RunOnce(delegate
							{
								RunOnce(0.1f, delegate
								{
									view.Set(view.data, content_list);
								});
							}, 1f / 60f);
						}).Start();
					}
				});
			}
			m_lockUI = false;
			view.HideLoadingUI(0f, p_without_animating: true);
			view.SetRatingsAvailable(p_available: false);
			SetupNavigationScrolling(p_screen);
			if (m_lockUI)
			{
				view.HideLoadingUI(0f);
				m_lockUI = false;
			}
		}

		private void SetupNavigationScrolling(UIScreen p_screen)
		{
			UINavigationScroll component = p_screen.GetComponent<UINavigationScroll>();
			if ((bool)component)
			{
				component.ResetScroll(p_force: true);
			}
			if (view.category != GameFlag.MapMultiGP)
			{
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
			}
		}
	}
}
