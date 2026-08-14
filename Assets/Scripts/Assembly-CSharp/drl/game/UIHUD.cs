using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UIHUD : UIScreen
	{
		public UIHUDMarkerLayer marker;

		public UIHUDTrainingLayer training;

		public UIControllerOverlay controller;

		public UIHUDRaceLayer race;

		public UIHUDCollectables collectables;

		public UIHUDCounter counter;

		public FadeComponent lowFPSWarning;

		public FadeComponent lowSignalWarning;

		public UIHUDTitle gameTitle;

		public UIHUDUserInfo userInfo;

		public FadeComponent standingsFade;

		public UIHUDTurtleMode turtleMode;

		public DRLStandingsView standings;

		public UIDroneDashboardController dashboard;

		public UIBatteryMeterController batteryMeterController;

		public UIHUDPhysicsController physics;

		public UIHUDTimeout timeout;

		public UIHUDDamageIndicator damage;

		public GameObject crosshair;

		public UIInfoTipListView hotkeys;

		public void SetStandingsCount(int p_count, bool p_has_positions)
		{
			if (this == null || base.gameObject == null || standings == null)
			{
				return;
			}
			ListComponent listField = standings.listField;
			listField.Clear();
			for (int i = 0; i < p_count; i++)
			{
				DRLStandingsItemView dRLStandingsItemView = listField.Push<DRLStandingsItemView>();
				if ((bool)dRLStandingsItemView)
				{
					dRLStandingsItemView.hasPosition = p_has_positions;
				}
			}
		}

		public void SetHotkeysEnabled(bool p_flag)
		{
			_ = (bool)hotkeys;
		}

		public void RefreshStandings(List<GamePlayerData> p_list)
		{
			if (this == null || base.gameObject == null || standings == null || p_list == null)
			{
				return;
			}
			p_list.Sort(delegate(GamePlayerData a, GamePlayerData b)
			{
				bool flag = a.type == GamePlayerType.Spectator;
				bool flag2 = b.type == GamePlayerType.Spectator;
				if (!flag && flag2)
				{
					return 1;
				}
				if (flag && !flag2)
				{
					return -1;
				}
				return (a.order >= b.order) ? 1 : (-1);
			});
			int num = 0;
			for (int num2 = 0; num2 < p_list.Count; num2++)
			{
				if (p_list[num2].type != GamePlayerType.Data)
				{
					num++;
				}
			}
			DRLStandingsView dRLStandingsView = standings;
			dRLStandingsView.Clear();
			dRLStandingsView.SetCount(num);
			for (int num3 = 0; num3 < p_list.Count; num3++)
			{
				GamePlayerData gamePlayerData = p_list[num3];
				bool p_bold = gamePlayerData.type == GamePlayerType.Human;
				DRLStandingsItemView dRLStandingsItemView = dRLStandingsView.Set(num3, gamePlayerData.color, gamePlayerData.photo, gamePlayerData.name, -1f, p_bold, gamePlayerData.playerId);
				if (!(dRLStandingsItemView == null))
				{
					dRLStandingsItemView.contentFade.alpha = ((gamePlayerData.type == GamePlayerType.Spectator) ? 0.5f : 1f);
				}
			}
		}
	}
}
