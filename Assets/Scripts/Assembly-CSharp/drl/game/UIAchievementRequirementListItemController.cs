using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIAchievementRequirementListItemController : Controller<DRLApp>
	{
		public MapData currentMap;

		public UIAchievementRequirementListItemView view => AssertLocal<UIAchievementRequirementListItemView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current.name != view.parentScreenName)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_target != view) && base.app.controller.game != null)
				{
					base.app.controller.game.model.type = GameFlag.Race;
				}
				break;
			case "fly.map-track-card@click":
				if (!(p_target != view))
				{
					DRLMap map = view.mapTrack.map;
					DRLMapTrack mapTrack = view.mapTrack;
					string text = "";
					bool flag = false;
					MapData mapData2 = null;
					_ = new object[7] { map.guid, mapTrack.guid, text, flag, map, mapTrack, mapData2 };
					new Object();
					base.app.arguments.Clear();
					base.app.arguments.game.type = GameFlag.Race;
					base.app.arguments.game.mode = GameFlag.SinglePlayer;
					base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
					Notify("maps.selection-complete", map.guid, mapTrack.guid, text, flag, map, mapTrack, mapData2);
				}
				break;
			case "community-maps.item.fly@click":
			{
				if (p_target != view)
				{
					break;
				}
				string mapGUID = view.mapGUID;
				MapData mapData = base.app.model.storage.maps.FindByGUID(mapGUID);
				if ((bool)(p_target as Component))
				{
					base.app.arguments.Clear();
					base.app.arguments.game.type = GameFlag.Race;
					base.app.arguments.game.mode = GameFlag.SinglePlayer;
					base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
					RunOnce(0.5f, delegate
					{
						mapData.Load(mapData.ToJson());
						currentMap = mapData;
						LoadCommunityMap(currentMap);
					});
				}
				break;
			}
			}
		}

		private void LoadCommunityMap(MapData p_mapData)
		{
			base.app.controller.LoadCustomTrackOverview(p_mapData);
		}
	}
}
