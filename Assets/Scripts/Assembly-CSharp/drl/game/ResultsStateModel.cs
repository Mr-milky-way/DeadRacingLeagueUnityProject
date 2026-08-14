using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ResultsStateModel : Model<DRLApp>
	{
		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public DataFlow data => parent.data;

		public List<DRLRaceResultData> list
		{
			get
			{
				return Serialize.FromJson<List<DRLRaceResultData>>(data.Contains("results-list") ? data.Get<string>("results-list") : ((string)data.Set("results-list", Serialize.ToJson(new List<DRLRaceResultData>()))));
			}
			set
			{
				string v = Serialize.ToJson((value == null) ? new List<DRLRaceResultData>() : value);
				data.Set("results-list", v);
				Refresh();
			}
		}

		public CampaignResultsModel campaign => AssertFind<CampaignResultsModel>("campaign");

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}

		public List<DRLRaceResultData> FindAll(string p_player_id, int p_order, GameFlag p_type, string p_campaign, string p_mission, string p_map, string p_track)
		{
			List<DRLRaceResultData> list = this.list;
			List<DRLRaceResultData> list2 = new List<DRLRaceResultData>();
			string text = (string.IsNullOrEmpty(p_player_id) ? base.app.model.service.backend.playerId : p_player_id);
			bool flag = text == " ";
			bool flag2 = p_type == GameFlag.None;
			bool flag3 = p_order < 0;
			bool flag4 = p_campaign == "";
			bool flag5 = p_mission == "";
			bool flag6 = p_map == "";
			bool flag7 = p_track == "";
			for (int i = 0; i < list.Count; i++)
			{
				DRLRaceResultData dRLRaceResultData = list[i];
				if ((flag || !(text != dRLRaceResultData.playerId)) && (flag2 || p_type == dRLRaceResultData.type) && (flag3 || p_order == dRLRaceResultData.order) && (flag4 || !(p_campaign != dRLRaceResultData.campaign)) && (flag5 || !(p_mission != dRLRaceResultData.mission)) && (flag6 || !(p_map != dRLRaceResultData.map)) && (flag7 || !(p_track != dRLRaceResultData.track)))
				{
					list2.Add(dRLRaceResultData);
				}
			}
			return list2;
		}

		public bool Match(DRLRaceResultData p_result, string p_player_id, int p_order, GameFlag p_type, string p_campaign, string p_mission, string p_map, string p_track)
		{
			string text = (string.IsNullOrEmpty(p_player_id) ? base.app.model.service.backend.playerId : p_player_id);
			bool num = text == " ";
			bool flag = p_type == GameFlag.None;
			bool flag2 = p_order < 0;
			bool flag3 = p_campaign == "";
			bool flag4 = p_mission == "";
			bool flag5 = p_map == "";
			bool flag6 = p_track == "";
			if (!num && text != p_result.playerId)
			{
				return false;
			}
			if (!flag && p_type != p_result.type)
			{
				return false;
			}
			if (!flag2 && p_order != p_result.order)
			{
				return false;
			}
			if (!flag3 && p_campaign != p_result.campaign)
			{
				return false;
			}
			if (!flag4 && p_mission != p_result.mission)
			{
				return false;
			}
			if (!flag5 && p_map != p_result.map)
			{
				return false;
			}
			if (!flag6 && p_track != p_result.track)
			{
				return false;
			}
			return true;
		}

		public List<DRLRaceResultData> FindAll(int p_order, GameFlag p_type, string p_campaign, string p_mission, string p_map, string p_track)
		{
			return FindAll(" ", p_order, p_type, p_campaign, p_mission, p_map, p_track);
		}

		public DRLRaceResultData FindByGUID(string p_guid, out int p_index)
		{
			List<DRLRaceResultData> list = this.list;
			p_index = -1;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].guid == p_guid)
				{
					p_index = i;
					return list[i];
				}
			}
			return null;
		}

		public DRLRaceResultData FindByGUID(string p_guid)
		{
			int p_index = 0;
			return FindByGUID(p_guid, out p_index);
		}

		public void UpdateResult(DRLRaceResultData p_data)
		{
			if (p_data == null)
			{
				return;
			}
			List<DRLRaceResultData> list = this.list;
			int p_index = -1;
			DRLRaceResultData dRLRaceResultData = FindByGUID(p_data.guid, out p_index);
			if (dRLRaceResultData == null)
			{
				list.Add(p_data);
				this.list = list;
				return;
			}
			dRLRaceResultData.Merge(p_data);
			if (p_index < 0)
			{
				Debug.LogWarning("ResultsStateModel> Update - Bad Index - guid[" + dRLRaceResultData.guid + "]");
				return;
			}
			list[p_index] = dRLRaceResultData;
			this.list = list;
		}

		public void RemoveByGUID(string p_guid)
		{
			List<DRLRaceResultData> list = this.list;
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				DRLRaceResultData dRLRaceResultData = list[i];
				if (dRLRaceResultData == null)
				{
					list.RemoveAt(i--);
					flag = true;
				}
				else if (dRLRaceResultData.guid == p_guid)
				{
					list.RemoveAt(i--);
					flag = true;
				}
			}
			if (flag)
			{
				this.list = list;
			}
		}

		public DRLRaceResultData Create(int p_order, string p_player_id, int p_score, int p_crashes = 0, GameFlag p_mode = GameFlag.SinglePlayer)
		{
			return new DRLRaceResultData
			{
				mode = p_mode,
				playerId = (string.IsNullOrEmpty(p_player_id) ? base.app.model.service.backend.playerId : p_player_id),
				crashes = p_crashes,
				score = p_score,
				order = p_order
			};
		}

		public DRLRaceResultData Create(string p_tournament, DRLMap p_map, DRLMapTrack p_track, int p_order, string p_player_id, int p_score, int p_crashes = 0, GameFlag p_mode = GameFlag.SinglePlayer)
		{
			DRLRaceResultData dRLRaceResultData = Create(p_order, p_player_id, p_score, p_crashes, p_mode);
			dRLRaceResultData.type = GameFlag.Race;
			dRLRaceResultData.map = (p_map ? p_map.guid : "");
			dRLRaceResultData.track = (p_track ? p_track.guid : "");
			return dRLRaceResultData;
		}
	}
}
