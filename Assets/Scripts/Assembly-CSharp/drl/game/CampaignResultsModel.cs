using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class CampaignResultsModel : Model<DRLApp>
	{
		public ResultsStateModel parent => AssertParent<ResultsStateModel>("results");

		public DataFlow data => parent.data;

		internal SerializedData attempts
		{
			get
			{
				return GetHashTable("campaigns-attempts-table");
			}
			set
			{
				SetHashTable("campaigns-attempts-table", value);
			}
		}

		internal SerializedData regions
		{
			get
			{
				return GetHashTable("campaigns-regions-table");
			}
			set
			{
				SetHashTable("campaigns-regions-table", value);
			}
		}

		internal SerializedData termsAccept
		{
			get
			{
				return GetHashTable("campaigns-terms-accept-table");
			}
			set
			{
				SetHashTable("campaigns-terms-accept-table", value);
			}
		}

		internal SerializedData newHighscore
		{
			get
			{
				return GetHashTable("campaigns-new-highscore-table");
			}
			set
			{
				SetHashTable("campaigns-new-highscore-table", value);
			}
		}

		internal SerializedData registerInfo
		{
			get
			{
				return GetHashTable("campaigns-register-info-table");
			}
			set
			{
				SetHashTable("campaigns-register-info-table", value);
			}
		}

		internal SerializedData GetHashTable(string k)
		{
			string text = (data.Contains(k) ? data.Get<string>(k) : "");
			if (!string.IsNullOrEmpty(text))
			{
				return Serialize.FromJson<SerializedData>(text);
			}
			return new SerializedData();
		}

		internal void SetHashTable(string k, SerializedData d)
		{
			string v = Serialize.ToJson((d == null) ? new SerializedData() : d);
			parent.data.Set(k, v);
			Refresh();
		}

		public List<DRLRaceResultData> FindAll(string p_player_id, DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return new List<DRLRaceResultData>();
			}
			return parent.FindAll(p_player_id, -1, GameFlag.Campaign, p_campaign.guid, "", "", "");
		}

		public List<DRLRaceResultData> FindAll(DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return new List<DRLRaceResultData>();
			}
			return parent.FindAll(" ", -1, GameFlag.Campaign, p_campaign.guid, "", "", "");
		}

		public List<DRLRaceResultData> FindAll(int p_order, DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return new List<DRLRaceResultData>();
			}
			return parent.FindAll(" ", p_order, GameFlag.Campaign, p_campaign.guid, "", "", "");
		}

		public DRLRaceResultData Find(string p_player_id, int p_order, DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return null;
			}
			List<DRLRaceResultData> list = parent.FindAll(p_player_id, p_order, GameFlag.Campaign, p_campaign.guid, "", "", "");
			if (list.Count > 0)
			{
				return list[0];
			}
			return null;
		}

		public DRLRaceResultData Find(int p_order, DRLCampaign p_campaign)
		{
			return Find(" ", p_order, p_campaign);
		}

		public void Clear(DRLCampaign p_campaign, bool p_increment_attempts)
		{
			List<DRLRaceResultData> list = parent.list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].campaign == p_campaign.guid)
				{
					list.RemoveAt(i--);
				}
			}
			parent.list = list;
			if (p_increment_attempts)
			{
				IncrementAttempts(p_campaign, 1);
			}
		}

		public void Clear(DRLCampaign p_campaign)
		{
			Clear(p_campaign, p_increment_attempts: false);
		}

		public void Remove(DRLCampaign p_campaign)
		{
			parent.RemoveByGUID(p_campaign.guid);
		}

		public int GetAttempts(DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return 0;
			}
			return attempts?.Get(p_campaign.guid, 0) ?? 0;
		}

		public void ResetAttempts(DRLCampaign p_campaign)
		{
			if ((bool)p_campaign)
			{
				SerializedData serializedData = attempts;
				if (serializedData != null)
				{
					serializedData.Set(p_campaign.guid, 0);
					attempts = serializedData;
				}
			}
		}

		public int IncrementAttempts(DRLCampaign p_campaign, int p_delta)
		{
			if (!p_campaign)
			{
				return 0;
			}
			SerializedData serializedData = attempts;
			if (serializedData == null)
			{
				return 0;
			}
			int num = serializedData.Get(p_campaign.guid, 0);
			num += p_delta;
			serializedData.Set(p_campaign.guid, num);
			attempts = serializedData;
			return num;
		}

		public string GetRegion(DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return "";
			}
			SerializedData serializedData = regions;
			if (serializedData == null)
			{
				return "";
			}
			return serializedData.Get(p_campaign.guid, "");
		}

		public void SetRegion(DRLCampaign p_campaign, string p_region)
		{
			if ((bool)p_campaign)
			{
				SerializedData serializedData = regions;
				if (serializedData != null)
				{
					serializedData.Set(p_campaign.guid, p_region);
					regions = serializedData;
				}
			}
		}

		public bool GetTermsAccept(DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return false;
			}
			return termsAccept?.Get(p_campaign.guid, d: false) ?? false;
		}

		public void SetTermsAccept(DRLCampaign p_campaign, bool p_flag)
		{
			if ((bool)p_campaign)
			{
				SerializedData serializedData = termsAccept;
				if (serializedData != null)
				{
					serializedData.Set(p_campaign.guid, p_flag);
					termsAccept = serializedData;
				}
			}
		}

		public CampaignRegisterInfo GetRegisterInfo(DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return null;
			}
			SerializedData serializedData = registerInfo;
			if (serializedData == null)
			{
				return null;
			}
			string text = serializedData.Get(p_campaign.guid, "");
			if (!string.IsNullOrEmpty(text))
			{
				return Serialize.FromJson<CampaignRegisterInfo>(text);
			}
			return new CampaignRegisterInfo();
		}

		public void SetRegisterInfo(DRLCampaign p_campaign, CampaignRegisterInfo p_data)
		{
			if ((bool)p_campaign)
			{
				SerializedData serializedData = termsAccept;
				if (serializedData != null)
				{
					string v = Serialize.ToJson((p_data == null) ? new CampaignRegisterInfo() : p_data);
					serializedData.Set(p_campaign.guid, v);
					registerInfo = serializedData;
				}
			}
		}

		public bool GetNewHighScore(DRLCampaign p_campaign)
		{
			if (!p_campaign)
			{
				return false;
			}
			return newHighscore?.Get(p_campaign.guid, d: false) ?? false;
		}

		public void SetNewHighScore(DRLCampaign p_campaign, bool p_flag)
		{
			if ((bool)p_campaign)
			{
				SerializedData serializedData = newHighscore;
				if (serializedData != null)
				{
					serializedData.Set(p_campaign.guid, p_flag);
					newHighscore = serializedData;
				}
			}
		}

		public int GetRaceIndex(string p_player_id, DRLCampaign p_campaign, out int p_phase, out int p_heat)
		{
			List<DRLRaceResultData> list = FindAll(p_player_id, p_campaign);
			int num = -1;
			int p_phase2 = 0;
			int p_heat2 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				_ = list[i];
				num = Mathf.Max(num, p_campaign.GetRaceIndex(i, out p_phase2, out p_heat2));
			}
			p_phase = p_phase2;
			p_heat = p_heat2;
			return num;
		}

		public int GetRaceIndex(DRLCampaign p_campaign, out int p_phase, out int p_heat)
		{
			return GetRaceIndex(" ", p_campaign, out p_phase, out p_heat);
		}

		public int GetRaceIndex(string p_player_id, DRLCampaign p_campaign)
		{
			int p_phase = 0;
			int p_heat = 0;
			return GetRaceIndex(p_player_id, p_campaign, out p_phase, out p_heat);
		}

		public int GetRaceIndex(DRLCampaign p_campaign)
		{
			return GetRaceIndex(" ", p_campaign);
		}

		public int GetRaceOrder(string p_player_id, DRLCampaign p_campaign)
		{
			return FindAll(p_player_id, p_campaign).Count;
		}

		public int GetRaceOrder(DRLCampaign p_campaign)
		{
			return GetRaceOrder(" ", p_campaign);
		}

		public List<DRLRaceResultData> GetResults(string p_player_id, DRLCampaign p_campaign)
		{
			return FindAll(p_player_id, p_campaign);
		}

		public List<DRLRaceResultData> GetResults(DRLCampaign p_campaign)
		{
			return GetResults(" ", p_campaign);
		}

		public List<string> GetRacePlayers(DRLCampaign p_campaign, out int p_player_idx)
		{
			List<string> list = new List<string>();
			List<DRLRaceResultData> list2 = FindAll(" ", p_campaign);
			string playerId = base.app.model.service.backend.playerId;
			p_player_idx = -1;
			for (int i = 0; i < list2.Count; i++)
			{
				string playerId2 = list2[i].playerId;
				if (!list.Contains(playerId2))
				{
					if (playerId2 == playerId)
					{
						p_player_idx = list.Count;
					}
					list.Add(playerId2);
				}
			}
			return list;
		}

		public List<DRLLeaderboardData> GetLeaderboards(DRLCampaign p_campaign)
		{
			List<DRLLeaderboardData> list = new List<DRLLeaderboardData>();
			if (!IsCampaignComplete(p_campaign))
			{
				return list;
			}
			int raceScore = GetRaceScore(p_campaign);
			DRLLeaderboardData dRLLeaderboardData = ServiceModel.CreateLeaderboardData(0, raceScore, 0, GameFlag.Campaign, p_multiplayer: false, ScoreType.TimeMin, null, null, p_campaign, null);
			string region = GetRegion(p_campaign);
			if (!string.IsNullOrEmpty(region))
			{
				dRLLeaderboardData.region = region;
			}
			list.Add(dRLLeaderboardData);
			List<DRLRaceResultData> results = GetResults(p_campaign);
			for (int i = 0; i < results.Count; i++)
			{
				DRLRaceResultData dRLRaceResultData = results[i];
				dRLLeaderboardData = ServiceModel.CreateRaceLeaderboardData(dRLRaceResultData.order, dRLRaceResultData.time, dRLRaceResultData.crashes, null);
				dRLLeaderboardData.group = dRLRaceResultData.campaign;
				dRLLeaderboardData.track = dRLRaceResultData.track;
				dRLLeaderboardData.map = dRLRaceResultData.map;
				dRLLeaderboardData.replayURL = dRLRaceResultData.replay;
				dRLLeaderboardData.customPhysics = false;
				dRLLeaderboardData.drlOfficial = true;
				dRLLeaderboardData.diameter = 6;
				if (!string.IsNullOrEmpty(region))
				{
					dRLLeaderboardData.region = region;
				}
				list.Add(dRLLeaderboardData);
			}
			return list;
		}

		public DRLRaceResultData SetNextCampaignResultScore(string p_player_id, DRLCampaign p_campaign, int p_score, int p_crashes = 0, RaceStatusType p_status = RaceStatusType.Success, GameFlag p_mode = GameFlag.SinglePlayer, bool p_remove_last = false)
		{
			List<DRLRaceResultData> list = FindAll(p_player_id, p_campaign);
			if (p_remove_last)
			{
				Debug.Log("CampaignResultsModel> remove-last[" + list.Count + "]");
				if (list.Count > 0)
				{
					list.RemoveAt(list.Count - 1);
					parent.list = list;
				}
			}
			int count = list.Count;
			int p_phase = 0;
			int p_heat = 0;
			if (p_campaign.IsComplete(count))
			{
				return null;
			}
			DRLCampaignRace race = p_campaign.GetRace(count, out p_phase, out p_heat);
			int num = p_campaign.races.IndexOf(race);
			Debug.Log("ResultsStateModel> SetNextCampaignResultScore - Campaign[" + p_campaign.guid + "/" + p_campaign.label + "] Results[" + count + "] Race[" + num + "] Phase[" + p_phase + "] Heat [" + p_heat + "] Status[" + p_status.ToString() + "]");
			DRLRaceResultData dRLRaceResultData = new DRLRaceResultData();
			dRLRaceResultData.type = GameFlag.Campaign;
			dRLRaceResultData.mode = p_mode;
			dRLRaceResultData.campaign = p_campaign.guid;
			dRLRaceResultData.playerId = (string.IsNullOrEmpty(p_player_id) ? base.app.model.service.backend.playerId : p_player_id);
			dRLRaceResultData.crashes = p_crashes;
			dRLRaceResultData.score = p_score;
			dRLRaceResultData.order = count;
			dRLRaceResultData.map = race.mapId;
			dRLRaceResultData.track = race.trackId;
			dRLRaceResultData.isCustomMap = race.isCustomMap;
			dRLRaceResultData.customMap = race.customMapId;
			dRLRaceResultData.status = (ResultStatusType)p_status;
			parent.UpdateResult(dRLRaceResultData);
			return dRLRaceResultData;
		}

		public DRLRaceResultData SetNextCampaignResultScore(DRLCampaign p_campaign, int p_score, int p_crashes = 0, RaceStatusType p_status = RaceStatusType.Success, GameFlag p_mode = GameFlag.SinglePlayer, bool p_remove_last = false)
		{
			return SetNextCampaignResultScore(" ", p_campaign, p_score, p_crashes, p_status, p_mode, p_remove_last);
		}

		public DRLRaceResultData SetNextCampaignResultTime(string p_player_id, DRLCampaign p_campaign, float p_time, int p_crashes = 0, RaceStatusType p_status = RaceStatusType.Success, GameFlag p_mode = GameFlag.SinglePlayer, bool p_remove_last = false)
		{
			return SetNextCampaignResultScore(p_player_id, p_campaign, Mathf.FloorToInt(p_time * 1000f), p_crashes, p_status, p_mode, p_remove_last);
		}

		public DRLRaceResultData SetNextCampaignResultTime(DRLCampaign p_campaign, float p_time, int p_crashes = 0, RaceStatusType p_status = RaceStatusType.Success, GameFlag p_mode = GameFlag.SinglePlayer, bool p_remove_last = false)
		{
			return SetNextCampaignResultScore(" ", p_campaign, Mathf.FloorToInt(p_time * 1000f), p_crashes, p_status, p_mode, p_remove_last);
		}

		public void UpdateResult(DRLRaceResultData p_result)
		{
			parent.UpdateResult(p_result);
		}

		public bool IsCampaignComplete(string p_player_id, DRLCampaign p_campaign)
		{
			List<DRLRaceResultData> list = FindAll(p_player_id, p_campaign);
			if (!p_campaign)
			{
				return false;
			}
			return p_campaign.IsComplete(list.Count);
		}

		public bool IsCampaignComplete(DRLCampaign p_campaign)
		{
			return IsCampaignComplete(" ", p_campaign);
		}

		public int GetRaceScore(string p_player_id, DRLCampaign p_campaign, int p_race_id)
		{
			int num = 0;
			List<DRLRaceResultData> list = FindAll(p_player_id, p_campaign);
			for (int i = 0; i < list.Count; i++)
			{
				DRLRaceResultData dRLRaceResultData = list[i];
				int raceIndex = p_campaign.GetRaceIndex(i);
				if (p_race_id < 0 || raceIndex == p_race_id)
				{
					num += dRLRaceResultData.score;
				}
			}
			return num;
		}

		public int GetRaceScore(DRLCampaign p_campaign, int p_race_id)
		{
			return GetRaceScore(" ", p_campaign, p_race_id);
		}

		public int GetRaceScore(string p_player_id, DRLCampaign p_campaign)
		{
			return GetRaceScore(p_player_id, p_campaign, -1);
		}

		public int GetRaceScore(DRLCampaign p_campaign)
		{
			return GetRaceScore(" ", p_campaign, -1);
		}

		public float GetRaceTime(string p_player_id, DRLCampaign p_campaign, int p_race_id)
		{
			return (float)GetRaceScore(p_player_id, p_campaign, p_race_id) / 1000f;
		}

		public float GetRaceTime(string p_player_id, DRLCampaign p_campaign)
		{
			return GetRaceTime(p_player_id, p_campaign, -1);
		}

		public float GetRaceTime(DRLCampaign p_campaign, int p_race_id)
		{
			return GetRaceTime(" ", p_campaign, p_race_id);
		}

		public float GetRaceTime(DRLCampaign p_campaign)
		{
			return GetRaceTime(" ", p_campaign, -1);
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}
	}
}
