using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class TuningStateModel : Model<DRLApp>
	{
		public float cameraMinFOV = 90f;

		public float cameraMaxFOV = 140f;

		private DRLCommunityTuneData m_currentTune;

		private Activity m_refresh_delayed;

		public SettingsStateModel parent => AssertParent<SettingsStateModel>("parent");

		public DataFlow data => parent.data;

		public List<FCProfileData> profiles
		{
			get
			{
				bool num = data.Contains("settings-fc-profiles");
				string text = "";
				if (num)
				{
					text = data.Get<string>("settings-fc-profiles");
				}
				else
				{
					List<FCProfileData> defaultProfiles = GetDefaultProfiles(5);
					text = (string)data.Set("settings-fc-profiles", Serialize.ToJson(defaultProfiles));
					Refresh();
				}
				return Serialize.FromJson<List<FCProfileData>>(text);
			}
			set
			{
				string text = Serialize.ToJson((value == null) ? GetDefaultProfiles(5) : value);
				if (value == null)
				{
					Debug.Log(text);
				}
				data.Set("settings-fc-profiles", text);
				Refresh();
			}
		}

		public List<DRLCommunityTuneData> tunes { get; set; }

		public DRLCommunityTuneData currentTune
		{
			get
			{
				if (m_currentTune == null)
				{
					currentTune = null;
				}
				return m_currentTune;
			}
			set
			{
				m_currentTune = value;
				if (m_currentTune == null)
				{
					if (tunes.Count > 0)
					{
						m_currentTune = tunes[0];
					}
					else
					{
						m_currentTune = new DRLCommunityTuneData();
					}
				}
			}
		}

		public string profileActiveGUID
		{
			get
			{
				bool num = data.Contains("settings-fc-profile-active-guid");
				FCProfileData fCProfileData = null;
				if (num)
				{
					string text = data.Get<string>("settings-fc-profile-active-guid");
					fCProfileData = GetProfile(text);
					if (fCProfileData != null)
					{
						return text;
					}
				}
				List<FCProfileData> list = profiles;
				fCProfileData = ((list.Count <= 0) ? null : list[0]);
				if (fCProfileData != null)
				{
					profileActiveGUID = fCProfileData.guid;
				}
				if (fCProfileData != null)
				{
					return fCProfileData.guid;
				}
				return "";
			}
			set
			{
				data.Set("settings-fc-profile-active-guid", value);
				Refresh();
			}
		}

		private List<FCProfileData> GetDefaultProfiles(int p_count)
		{
			List<FCProfileData> list = new List<FCProfileData>();
			for (int i = 0; i < p_count; i++)
			{
				list.Add(new FCProfileData());
			}
			return list;
		}

		public void AddTune(DRLCommunityTuneData p_data)
		{
			if (p_data != null && GetTune(tunes, p_data.guid) == null)
			{
				tunes.Add(p_data);
			}
		}

		public void RemoveTune(DRLCommunityTuneData p_data)
		{
			if (p_data != null && tunes != null)
			{
				tunes.RemoveAll((DRLCommunityTuneData it) => it != null && it.guid == p_data.guid);
			}
		}

		public DRLCommunityTuneData GetTune(string p_guid)
		{
			return GetTune(tunes, p_guid);
		}

		public void UpdateTune(DRLCommunityTuneData p_data)
		{
			for (int i = 0; i < tunes.Count; i++)
			{
				if (tunes[i].guid == p_data.guid)
				{
					tunes[i] = p_data;
				}
			}
		}

		protected DRLCommunityTuneData GetTune(List<DRLCommunityTuneData> p_list, string p_guid)
		{
			if (p_guid == null)
			{
				return null;
			}
			return p_list.Find((DRLCommunityTuneData it) => it.guid == p_guid);
		}

		public void AddProfile(FCProfileData p_data)
		{
			if (p_data != null)
			{
				List<FCProfileData> list = profiles;
				if (GetProfile(list, p_data.guid) == null)
				{
					list.Add(p_data);
					profiles = list;
				}
			}
		}

		public void RemoveProfile(FCProfileData p_data)
		{
			List<FCProfileData> list = profiles;
			list.RemoveAll((FCProfileData it) => it.guid == p_data.guid);
			profiles = list;
		}

		public FCProfileData GetProfile(int p_index)
		{
			List<FCProfileData> list = profiles;
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= list.Count)
			{
				return null;
			}
			return list[p_index];
		}

		public FCProfileData GetProfile(string p_guid)
		{
			return GetProfile(profiles, p_guid);
		}

		public void UpdateProfile(FCProfileData p_data)
		{
			List<FCProfileData> list = profiles;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].guid == p_data.guid)
				{
					list[i] = p_data;
				}
			}
			profiles = list;
		}

		protected FCProfileData GetProfile(List<FCProfileData> p_list, string p_guid)
		{
			if (p_guid == null)
			{
				return null;
			}
			return p_list.Find((FCProfileData it) => it.guid == p_guid);
		}

		public bool IsActive(FCProfileData p_data)
		{
			if (p_data != null)
			{
				return p_data.guid == profileActiveGUID;
			}
			return false;
		}

		public FCProfileData GetActive()
		{
			int activeIndex = GetActiveIndex();
			List<FCProfileData> list = profiles;
			if (activeIndex >= 0)
			{
				return list[activeIndex];
			}
			return null;
		}

		public int GetActiveIndex()
		{
			string text = profileActiveGUID;
			List<FCProfileData> list = profiles;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].guid == text)
				{
					return i;
				}
			}
			return -1;
		}

		public void UpdateCameraDelayed(float p_tilt = -1f, float p_fov = -1f)
		{
			if (m_refresh_delayed != null)
			{
				m_refresh_delayed.Stop();
			}
			m_refresh_delayed = Activity.RunOnce(delegate
			{
				FCProfileData active = GetActive();
				active.tilt = ((p_tilt < 0f) ? active.tilt : p_tilt);
				active.fov = ((p_fov < 0f) ? active.fov : p_fov);
				UpdateProfile(active);
				Notify("settings.tuning.profile.save", active);
			}, 1f);
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
