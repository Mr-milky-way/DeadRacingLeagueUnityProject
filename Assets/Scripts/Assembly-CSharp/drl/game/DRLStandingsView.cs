using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLStandingsView : View<DRLApp>
	{
		public ListComponent listField;

		private int m_count;

		private Dictionary<string, Texture> cachePhotos = new Dictionary<string, Texture>();

		public int count => m_count;

		private FadeComponent fade => AssertLocal<FadeComponent>("fade");

		protected void Awake()
		{
			Clear();
		}

		public DRLStandingsItemView Get(int p_id)
		{
			if (p_id < 0)
			{
				return null;
			}
			if (p_id >= listField.Count)
			{
				return null;
			}
			return listField.Get<DRLStandingsItemView>(p_id);
		}

		public void SetCount(int p_count)
		{
			int num = listField.Count;
			for (int i = 0; i < num; i++)
			{
				DRLStandingsItemView dRLStandingsItemView = Get(i);
				if ((bool)dRLStandingsItemView)
				{
					m_count = p_count;
					if (i >= p_count)
					{
						dRLStandingsItemView.SetVisible(p_flag: false);
						dRLStandingsItemView.gameObject.SetActive(value: false);
						continue;
					}
					dRLStandingsItemView.position = i;
					dRLStandingsItemView.hasPosition = p_count > 1;
					dRLStandingsItemView.profileName = "";
					dRLStandingsItemView.time = 0f;
					dRLStandingsItemView.backgroundColor = DRLColor.red;
					dRLStandingsItemView.SetVisible(p_flag: true);
					dRLStandingsItemView.gameObject.SetActive(value: true);
				}
			}
		}

		public void Clear()
		{
			int num = listField.Count;
			for (int i = 0; i < num; i++)
			{
				DRLStandingsItemView dRLStandingsItemView = Get(i);
				if ((bool)dRLStandingsItemView)
				{
					dRLStandingsItemView.playerId = "";
					dRLStandingsItemView.position = i;
					dRLStandingsItemView.profileName = "";
					dRLStandingsItemView.time = 0f;
					dRLStandingsItemView.backgroundColor = DRLColor.red;
					dRLStandingsItemView.SetVisible(p_flag: false);
				}
			}
		}

		public void Fade(bool p_flag, float p_duration, float p_delay, float p_step)
		{
			Debug.Log("<color=red>DRLStandingsView> </color> Fade " + p_flag + "\n" + StackTraceUtility.ExtractStackTrace());
			fade.Fade(p_flag ? 1f : 0f, 0f);
			int num = listField.Count;
			for (int i = 0; i < num; i++)
			{
				DRLStandingsItemView dRLStandingsItemView = Get(i);
				if ((bool)dRLStandingsItemView)
				{
					float num2 = i;
					num2 *= p_step;
					num2 += p_delay;
					dRLStandingsItemView.Fade(p_flag, p_duration, num2);
				}
			}
		}

		public void Fade(bool p_flag, float p_duration, float p_step = 0.02f)
		{
			Fade(p_flag, p_duration, 0f, p_step);
		}

		public DRLStandingsItemView Set(int p_id, Color p_color, Texture p_photo, string p_name, float p_time, bool p_bold, string p_player_id, bool p_has_damage = false)
		{
			DRLStandingsItemView dRLStandingsItemView = Get(p_id);
			if (!dRLStandingsItemView)
			{
				return dRLStandingsItemView;
			}
			if (dRLStandingsItemView.playerId == p_player_id && dRLStandingsItemView.time == p_time)
			{
				return dRLStandingsItemView;
			}
			p_color *= (((p_id & 1) == 0) ? 1f : 0.8f);
			p_color.a = 1f;
			dRLStandingsItemView.playerId = p_player_id;
			dRLStandingsItemView.backgroundColor = p_color;
			dRLStandingsItemView.SetDamageIndicator(p_has_damage);
			if (!cachePhotos.ContainsKey(p_player_id) || cachePhotos[p_player_id] != dRLStandingsItemView.profilePhoto)
			{
				dRLStandingsItemView.profilePhoto = p_photo;
			}
			dRLStandingsItemView.profileName = ((p_name.Length <= 16) ? p_name : p_name.Substring(0, 16));
			dRLStandingsItemView.time = p_time;
			dRLStandingsItemView.bold = p_bold;
			return dRLStandingsItemView;
		}

		public DRLStandingsItemView Set(int p_id, Color p_color, string p_photo_url, string p_name, float p_time, bool p_bold, string p_player_id)
		{
			Debug.Log("<color=red>DRLStandingsView> </color> Set() " + StackTraceUtility.ExtractStackTrace());
			if (cachePhotos.ContainsKey(p_player_id))
			{
				return Set(p_id, p_color, cachePhotos[p_player_id], p_name, p_time, p_bold, p_player_id);
			}
			Texture2D tex = null;
			base.app.model.service.GetPlayerAvatar(p_player_id, delegate(Texture2D p_result)
			{
				if ((bool)p_result)
				{
					tex = p_result;
				}
			});
			cachePhotos.Add(p_player_id, tex);
			return Set(p_id, p_color, tex, p_name, p_time, p_bold, p_player_id);
		}

		public DRLStandingsItemView Set(int p_id, Color p_color, Texture p_photo, string p_name, float p_time, string p_steamId)
		{
			return Set(p_id, p_color, p_photo, p_name, p_time, p_bold: false, p_steamId);
		}

		public DRLStandingsItemView SetTime(int p_id, float p_time)
		{
			Debug.Log("<color=red>DRLStandingsView> </color> SetTime() " + StackTraceUtility.ExtractStackTrace());
			DRLStandingsItemView dRLStandingsItemView = Get(p_id);
			if (!dRLStandingsItemView)
			{
				return dRLStandingsItemView;
			}
			dRLStandingsItemView.time = p_time;
			return dRLStandingsItemView;
		}

		public void Refresh(List<GamePlayerData> p_players, bool p_clear = true, bool p_dnf = false, bool p_displayDNF = true)
		{
			int num = 0;
			bool flag = base.app.model.network.room != null;
			if (flag)
			{
				num = base.app.model.network.room.Racers.Count;
				for (int i = 0; i < p_players.Count; i++)
				{
					if (p_players[i].type == GamePlayerType.Ghost)
					{
						num++;
					}
				}
			}
			if (num >= p_players.Count || !flag)
			{
				num = p_players.Count;
			}
			if (p_clear || count != num)
			{
				Debug.Log("<color=red>DRLStandingsView> </color> Refresh(): will_clear was set true " + StackTraceUtility.ExtractStackTrace());
				Clear();
				SetCount(num);
			}
			for (int j = 0; j < p_players.Count; j++)
			{
				GamePlayerData it = p_players[j];
				if (!it.isRacer)
				{
					continue;
				}
				if (base.app.model.network.room != null && (it.type == GamePlayerType.Human || it.type == GamePlayerType.Network) && base.app.model.network.room.Racers.All((NetworkActor o) => o.PlayerId != it.playerId))
				{
					Debug.Log("<color=red>DRLStandingsView> </color> Refresh: player IDs don't match " + it.playerId + " " + StackTraceUtility.ExtractStackTrace());
					continue;
				}
				float p_time = it.raceTime;
				RaceStatusType raceStatus = it.raceStatus;
				if ((uint)raceStatus > 1u)
				{
					p_time = -1f;
				}
				bool p_bold = it.type == GamePlayerType.Human;
				bool p_has_damage = it.isRacer && it.raceStatus == RaceStatusType.Running && base.app.model.network != null && base.app.model.network.HasDamage(it.id);
				DRLStandingsItemView dRLStandingsItemView = Set(j, it.color, it.photo, it.upperName, p_time, p_bold, it.playerId, p_has_damage);
				if (!(dRLStandingsItemView == null))
				{
					if (raceStatus == RaceStatusType.Crash)
					{
						dRLStandingsItemView.timeLabel = "DNF";
					}
					if (raceStatus == RaceStatusType.Timeout)
					{
						dRLStandingsItemView.timeLabel = "DNF";
					}
					if (raceStatus == RaceStatusType.Quit)
					{
						dRLStandingsItemView.timeLabel = "DNF";
					}
					if (raceStatus == RaceStatusType.Forfeit)
					{
						dRLStandingsItemView.timeLabel = "DNF";
					}
					if (p_dnf)
					{
						dRLStandingsItemView.timeLabel = "DNF";
					}
					dRLStandingsItemView.position = j;
					if (base.app.inVirtualSeason && base.app.inTournament && !p_displayDNF && dRLStandingsItemView.timeField.text == "DNF")
					{
						dRLStandingsItemView.SetVisible(p_flag: false);
						dRLStandingsItemView.gameObject.SetActive(value: false);
					}
				}
			}
		}

		private void OnDestroy()
		{
			Debug.Log("<color=red>DRLStandingsView> </color> OnDestroy() " + StackTraceUtility.ExtractStackTrace());
			Dictionary<string, Texture> dictionary = cachePhotos;
			if (dictionary == null)
			{
				return;
			}
			foreach (KeyValuePair<string, Texture> item in dictionary)
			{
				if ((bool)item.Value)
				{
					UnityEngine.Object.DestroyImmediate(item.Value, allowDestroyingAssets: true);
				}
			}
			dictionary.Clear();
		}

		public void Refresh(List<UILeaderboardItemView> pPlayers, bool p_clear, bool p_dnf, bool p_displayDNF = true)
		{
			int num = 0;
			if (base.app.model.network.room != null)
			{
				num = base.app.model.network.room.Racers.Count;
				for (int i = 0; i < pPlayers.Count; i++)
				{
					num++;
				}
			}
			if (num >= pPlayers.Count)
			{
				num = pPlayers.Count;
			}
			if (p_clear || count != num)
			{
				Debug.Log("<color=red>DRLStandingsView> </color> Refresh(): will_clear was set true " + StackTraceUtility.ExtractStackTrace());
				Clear();
				SetCount(num);
			}
			for (int j = 0; j < pPlayers.Count; j++)
			{
				UILeaderboardItemView uILeaderboardItemView = pPlayers[j];
				float p_time = uILeaderboardItemView.data.scoreSeconds;
				RaceStatusType raceStatusFlag = uILeaderboardItemView.data.raceStatusFlag;
				if ((uint)raceStatusFlag > 1u)
				{
					p_time = -1f;
				}
				bool p_bold = false;
				bool p_has_damage = uILeaderboardItemView.data.raceStatusFlag == RaceStatusType.Running && base.app.model.network != null && base.app.model.network.HasDamage(Convert.ToInt32(uILeaderboardItemView.data.id));
				DRLStandingsItemView itv = Set(j, uILeaderboardItemView.data.profileColor, null, uILeaderboardItemView.data.username, p_time, p_bold, uILeaderboardItemView.data.playerId, p_has_damage);
				if (itv == null)
				{
					continue;
				}
				if (raceStatusFlag == RaceStatusType.Crash)
				{
					itv.timeLabel = "DNF";
				}
				if (raceStatusFlag == RaceStatusType.Timeout)
				{
					itv.timeLabel = "DNF";
				}
				if (raceStatusFlag == RaceStatusType.Quit)
				{
					itv.timeLabel = "DNF";
				}
				if (raceStatusFlag == RaceStatusType.Forfeit)
				{
					itv.timeLabel = "DNF";
				}
				if (p_dnf)
				{
					itv.timeLabel = "DNF";
				}
				itv.position = j;
				itv.profileNameField.text = uILeaderboardItemView.data.profileName.ToUpper();
				itv.photoField.texture = uILeaderboardItemView.profilePhotoField.mainTexture;
				itv.profilePhoto = uILeaderboardItemView.profilePhotoField.mainTexture;
				Web.Load(uILeaderboardItemView.data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_progress < 1f) && !(this == null))
					{
						itv.profilePhoto = p_result;
					}
				});
				if (base.app.inVirtualSeason && base.app.inTournament && !p_displayDNF && itv.timeField.text == "DNF")
				{
					itv.SetVisible(p_flag: false);
					itv.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
