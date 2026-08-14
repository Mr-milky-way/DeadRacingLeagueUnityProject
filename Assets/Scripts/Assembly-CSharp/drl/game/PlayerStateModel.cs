using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class PlayerStateModel : Model<DRLApp>
	{
		[SerializeField]
		private List<string> m_datakey_change_list;

		private List<DRLMapFavoriteData> m_favoriteMaps;

		private List<string> m_blockedUsers;

		private static string m_podium_id = "PD-a6d";

		private static DronePodium m_podium;

		private Activity m_refresh_timer;

		public DataFlow data => AssertLocal<DataFlow>("data");

		public List<string> dataKeyChangeList
		{
			get
			{
				if (m_datakey_change_list != null)
				{
					return m_datakey_change_list;
				}
				return m_datakey_change_list = new List<string>();
			}
		}

		public SettingsStateModel settings => AssertFind<SettingsStateModel>("settings");

		public ResultsStateModel results => AssertFind<ResultsStateModel>("results");

		public ProfileStateModel profile => AssertFind<ProfileStateModel>("profile");

		public ProgressionStateModel progression => AssertFind<ProgressionStateModel>("progression");

		public GarageStateModel garage => AssertFind<GarageStateModel>("garage");

		public CircuitStateModel circuits => AssertFind<CircuitStateModel>("circuits");

		public List<DRLMapFavoriteData> favoriteMaps
		{
			get
			{
				if (m_favoriteMaps == null)
				{
					m_favoriteMaps = new List<DRLMapFavoriteData>();
				}
				if (!data.Contains("maps-favorite"))
				{
					return m_favoriteMaps;
				}
				string p_data = data.Get<string>("maps-favorite");
				m_favoriteMaps = Serialize.FromJson<List<DRLMapFavoriteData>>(p_data);
				return m_favoriteMaps;
			}
			set
			{
				if (value != null)
				{
					m_favoriteMaps = value;
					string v = Serialize.ToJson(value);
					data.Set("maps-favorite", v);
					Refresh();
				}
			}
		}

		public OnboardingStateModel onboarding => AssertFind<OnboardingStateModel>("onboarding");

		public List<string> blockedUsers
		{
			get
			{
				if (m_blockedUsers == null)
				{
					m_blockedUsers = new List<string>();
				}
				if (!data.Contains("blocked-users"))
				{
					return m_blockedUsers;
				}
				string p_data = data.Get<string>("blocked-users");
				m_blockedUsers = new List<string>();
				m_blockedUsers = Serialize.FromJson<List<string>>(p_data);
				return m_blockedUsers;
			}
			set
			{
				if (value != null)
				{
					m_blockedUsers = value;
					string v = Serialize.ToJson(value);
					data.Set("blocked-users", v);
					Refresh();
				}
			}
		}

		public string podiumId
		{
			get
			{
				return m_podium_id;
			}
			set
			{
				m_podium_id = value;
				m_podium = base.app.model.storage.library.FindByGUID<DronePodium>(m_podium_id);
				if (!m_podium)
				{
					Debug.LogWarning("PlayerStateModel> Podium [" + m_podium_id + "] not found!");
				}
			}
		}

		public DronePodium podium
		{
			get
			{
				if (!m_podium)
				{
					m_podium = base.app.model.storage.library.FindByGUID<DronePodium>("PD-a6d");
				}
				return m_podium;
			}
		}

		public GamePlayerData playerData
		{
			get
			{
				GamePlayerData obj = new GamePlayerData
				{
					type = GamePlayerType.Human,
					platformId = profile.platformId,
					playerId = profile.playerId,
					name = profile.username
				};
				obj.upperName = obj.name.ToUpper();
				obj.color = profile.color;
				obj.photo = ((profile.photo is Texture2D) ? ((Texture2D)profile.photo) : null);
				obj.podiumId = podiumId;
				return obj;
			}
		}

		public string systemInfo
		{
			set
			{
				string k = "system-info_" + OS.prefix + "_" + SystemInfo.graphicsDeviceID;
				data.Set(k, value);
				Refresh();
			}
		}

		public FCMode activeFCMode
		{
			get
			{
				return (FCMode)data.Get("fcmode-active", 3);
			}
			set
			{
				data.Set("fcmode-active", (int)value);
				Refresh();
			}
		}

		public FCMode activeFCModeMissions
		{
			get
			{
				return (FCMode)data.Get("fcmode-active-missions", 0);
			}
			set
			{
				data.Set("fcmode-active-missions", (int)value);
				Refresh();
			}
		}

		public CloudRegionCode selectedNetworkRegion
		{
			get
			{
				return (CloudRegionCode)data.Get("network-server-region", 4);
			}
			set
			{
				data.Set("network-server-region", (int)value);
				Refresh();
			}
		}

		public CloudRegionCode connectedNetworkRegion
		{
			get
			{
				return (CloudRegionCode)data.Get("network-connected-region", 4);
			}
			set
			{
				data.Set("network-connected-region", (int)value);
				Refresh();
			}
		}

		public PreferedLanguage preferedLanguage
		{
			get
			{
				return (PreferedLanguage)data.Get("settings-language", 0);
			}
			set
			{
				data.Set("settings-language", (int)value);
				Refresh();
			}
		}

		public bool paywallDismiss
		{
			get
			{
				return data.Get("profile-paywall-dismiss", d: false);
			}
			set
			{
				data.Set("profile-paywall-dismiss", value);
				Refresh();
			}
		}

		public bool physicsIntro
		{
			get
			{
				return data.Get("profile-physics-intro", d: false);
			}
			set
			{
				data.Set("profile-physics-intro", value);
				Refresh();
			}
		}

		public List<PollResultModel> polls
		{
			get
			{
				return Serialize.FromJson<List<PollResultModel>>(data.Get("profile-polls", "[]"));
			}
			set
			{
				string v = ((value == null) ? "[]" : Serialize.ToJson(value));
				data.Set("profile-polls", v);
				Refresh();
			}
		}

		public bool physicsTuneWarning
		{
			get
			{
				return data.Get("physics-tune-warning", d: true);
			}
			set
			{
				data.Set("physics-tune-warning", value);
				Refresh();
			}
		}

		public bool dmvWelcomeScreen
		{
			get
			{
				return data.Get("dmv-welcome-screen", d: false);
			}
			set
			{
				data.Set("dmv-welcome-screen", value);
				Refresh();
			}
		}

		public int userRank
		{
			get
			{
				return data.Get("profile-user-rank", 0);
			}
			set
			{
				data.Set("profile-user-rank", value);
				Refresh();
				Notify(0.5f, "missions.dmv.rank.updated");
			}
		}

		public float dmvUserTotalTime
		{
			get
			{
				return data.Get("dmv-total-time", 0f);
			}
			set
			{
				data.Set("dmv-total-time", value);
				Refresh();
			}
		}

		public void SetData(string p_key, object p_value)
		{
			List<string> list = dataKeyChangeList;
			if (!list.Contains(p_key))
			{
				list.Add(p_key);
			}
			data.Set(p_key, p_value);
			Refresh();
		}

		public void Refresh()
		{
			if (base.validContext && base.app.model.storage.state.ready)
			{
				if (m_refresh_timer != null)
				{
					m_refresh_timer.Stop();
				}
				m_refresh_timer = Activity.RunOnce(delegate
				{
					Notify("storage.state@refresh");
				}, 1f);
			}
		}

		public void ReloadState()
		{
			if (!base.validContext || !base.app.model.storage.state.ready)
			{
				return;
			}
			base.app.model.service.State(delegate(DRLServiceResult p_result)
			{
				if (base.validContext && (p_result == null || !p_result.success))
				{
					Debug.LogWarning("PlayerStateModel> Failed to reload player state.");
				}
			});
		}
	}
}
